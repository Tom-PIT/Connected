using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Storage;
using TomPIT.Worker.Services;

namespace TomPIT.Worker.Workers
{
	internal sealed class Queue : IDisposable
	{
		private string _bufferKey = null;
		public Queue(IQueueMessage message)
		{
			Message = message;

			Initialize();
		}

		private bool IsDisposed { get; set; }
		private IQueueWorker Worker { get; }
		public IQueueMiddleware HandlerInstance { get; private set; }
		public string QueueName { get; private set; }
		private IMicroServiceContext Context { get; set; }
		public bool IsValid { get; private set; }
		private Guid MicroService { get; set; }
		private Type QueueType { get; set; }
		private string Arguments { get; set; }
		public IQueueMessage Message { get; }
		private void Initialize()
		{
			var message = JsonConvert.DeserializeObject(Message.Message) as JObject;
			var component = message.Required<Guid>("component");
			var worker = message.Required<string>("worker");

			Arguments = message.Optional<string>("arguments", null);

			if (MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component) is not IQueueConfiguration configuration)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(Invoke), $"{SR.ErrCannotFindConfiguration} ({component})", nameof(QueueWorkerJob));
				return;
			}

			if (configuration.Workers.FirstOrDefault(f => string.Equals(f.Name, worker, StringComparison.OrdinalIgnoreCase)) is not IQueueWorker workerConfiguration)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(Invoke), $"{SR.ErrQueueWorkerNotFound} ({component})", nameof(QueueWorkerJob));
				return;
			}

			Context = new MicroServiceContext(configuration.MicroService());
			MicroService = Worker.Configuration().MicroService();
			QueueType = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(MicroService, Worker, Worker.Name);
			IsValid = true;
		}

		public bool Invoke(ProcessBehavior behavior)
		{
			switch (behavior)
			{
				case ProcessBehavior.Parallel:
					if (!ProcessParallel())
						return false;
					break;
				case ProcessBehavior.Queued:
					ProcessQueued();
					break;
				default:
					throw new NotSupportedException();
			}

			return true;
		}

		private bool ProcessParallel()
		{
			var att = QueueType.FindAttribute<ProcessBehaviorAttribute>();

			if (att?.Behavior == ProcessBehavior.Queued)
			{
				QueueName = att.QueueName;
				return false;
			}

			return true;
		}

		private void ProcessQueued()
		{
			HandlerInstance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IQueueMiddleware>(Context, QueueType, Arguments);

			HandlerInstance.Invoke();
		}

		private void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Context?.Dispose();
					Context = null;
				}

				IsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
