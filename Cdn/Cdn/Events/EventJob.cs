using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Messaging;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Cdn.Events
{
    internal class EventJob : DispatcherJob<IEventQueueMessage>
    {
        public EventJob(IDispatcher<IEventQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
        {
        }

        private string MicroService { get; set; }
        private string Event { get; set; }
        protected override void DoWork(IEventQueueMessage item)
        {
            var timeout = new TimeoutTask(() =>
            {
                Delay(item.PopReceipt, 300);

                return Task.CompletedTask;
            }, TimeSpan.FromMinutes(4), Cancel);


            timeout.Start();

            try
            {
                if (!Invoke(item))
                    return;
            }
            finally
            {
                timeout.Stop();
                timeout = null;
            }

            var sw = Stopwatch.StartNew();

            Instance.SysProxy.Management.Events.Complete(item.PopReceipt);

            sw.Stop();

            Debug.WriteLine($"{Event} took {sw.ElapsedMilliseconds} to send completed notification.");
        }

        private static void Delay(Guid popReceipt, int delay)
        {
            Instance.SysProxy.Management.Events.Ping(popReceipt, delay);
        }

        private bool Invoke(IEventQueueMessage message)
        {
            if (MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(message.MicroService) is not IMicroService ms)
                return true;

            MicroService = ms.Name;
            Event = message.Name;

            using var ctx = new MicroServiceContext(ms);
            var responses = new List<IOperationResponse>();
            IDistributedEventMiddleware eventInstance = null;

            if (string.Compare(message.Name, "$", true) != 0)
            {
                var eventName = $"{ms.Name}/{message.Name}";
                eventInstance = CreateEventInstance(ctx, message);

                if (eventInstance != null)
                {
                    if (Owner.Behavior == ProcessBehavior.Parallel)
                    {
                        var att = eventInstance.GetType().FindAttribute<ProcessBehaviorAttribute>();

                        if (att?.Behavior == ProcessBehavior.Queued)
                        {
                            Owner.Enqueue(att.QueueName, message);
                            return false;
                        }
                    }

                    var args = new DistributedEventInvokingArgs();

                    eventInstance.Invoking(args);

                    switch (args.Result)
                    {
                        case EventInvokingResult.Cancel:
                            return true;
                        case EventInvokingResult.Delay:
                            Delay(message.PopReceipt, args.Delay == TimeSpan.Zero ? 60 : Convert.ToInt32(args.Delay.TotalSeconds));
                            return false;
                    }

                    eventInstance.Invoke();
                }

                var targets = EventHandlers.Query(eventName);

                if (targets != null)
                {
                    foreach (var target in targets)
                    {
                        if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(target.Item2) is IEventBindingConfiguration configuration))
                            continue;

                        Parallel.ForEach(configuration.Events,
                            (i) =>
                            {
                                if (string.Compare(eventName, i.Event, true) == 0)
                                {
                                    var result = Invoke(message, i);

                                    if (result != null && result.Count > 0)
                                    {
                                        lock (responses)
                                        {
                                            responses.AddRange(result);
                                        }
                                    }
                                }
                            });
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(message.Callback))
                Callback(message, responses);

            if (eventInstance != null)
                eventInstance.Invoked();

            Notify(ms, message, responses);

            return true;
        }

        private void Notify(IMicroService microService, Cdn.IEventQueueMessage message, List<IOperationResponse> responses)
        {
            if (responses.Count > 0)
            {
                foreach (var response in responses)
                {
                    if (response.Result == ResponseResult.Objection)
                        return;
                }
            }

            MiddlewareDescriptor.Current.Tenant.GetService<IEventHubService>().NotifyAsync(new EventHubNotificationArgs($"{microService.Name}/{message.Name}", message.Arguments));
        }

        private void Callback(Cdn.IEventQueueMessage message, List<IOperationResponse> responses)
        {
            using var ctx = new MicroServiceContext(new Guid(message.Callback.Split('/')[0]));

            if (!string.IsNullOrWhiteSpace(message.Arguments))
            {
                var argumentsState = JObject.Parse(message.Arguments);
                if (argumentsState.TryGetValue("user$", out var userToken))
                {
                    var userId = userToken.Value<string>();
                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        ctx.Impersonate(userId);
                    }
                }
            }

            var descriptor = ComponentDescriptor.Api(ctx, message.Callback);

            try
            {
                descriptor.Validate();
            }
            catch (RuntimeException ex)
            {
                TomPITException.Unwrap(this, ex).LogError(LogCategories.Cdn);
            }

            var op = descriptor.Configuration.Operations.FirstOrDefault(f => f.Id == new Guid(descriptor.Element));

            if (op == null)
                return;

            var instance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(ctx, op, message.Arguments, op.Name);

            ReflectionExtensions.SetPropertyValue(instance, nameof(IDistributedOperation.OperationTarget), DistributedOperationTarget.InProcess);

            if (responses != null && responses.Count > 0)
                instance.Responses.AddRange(responses);

            instance.Invoke();
        }

        private List<IOperationResponse> Invoke(Cdn.IEventQueueMessage message, IEventBinding i)
        {
            if (string.IsNullOrEmpty(i.Name))
                return null;

            using var context = new MicroServiceContext(i.Configuration().MicroService());
            var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(i.Configuration().MicroService(), i, i.Name, false);

            if (type == null)
            {
                context.Services.Diagnostic.Warning(message.Name, $"{SR.ErrTypeExpected} ({i.Name})", nameof(Invoke));
                return null;
            }

            try
            {
                var handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IEventMiddleware>(context, type, message.Arguments);

                handler.Invoke(message.Name);

                return handler.Responses;
            }
            catch (Exception ex)
            {
                TomPITException.Unwrap(this, ex).LogError(LogCategories.Cdn);
            }

            return null;
        }

        protected override void OnError(Cdn.IEventQueueMessage message, Exception ex)
        {
            if (ex is MiddlewareValidationException mw)
                mw.LogWarning(LogCategories.Cdn);

            TomPITException.Unwrap(this, ex).LogError(LogCategories.Cdn);

            Instance.SysProxy.Management.Events.Complete(message.PopReceipt);
        }

        private IDistributedEventMiddleware CreateEventInstance(IMicroServiceContext context, IEventQueueMessage message)
        {
            var compiler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>();
            var descriptor = ComponentDescriptor.DistributedEvent(context, $"{context.MicroService.Name}/{message.Name}");

            descriptor.Validate();

            if (descriptor.Configuration is null)
                return null;

            if (descriptor.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0) is not IDistributedEvent ev)
                throw new RuntimeException($"{SR.ErrDistributedEventNotFound} ({message.Name})");

            if (compiler.ResolveType(context.MicroService.Token, ev, ev.Name, false) is not Type type)
                return null;

            return compiler.CreateInstance<IDistributedEventMiddleware>(context, type, message.Arguments);
        }
    }
}
