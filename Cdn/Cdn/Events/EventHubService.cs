using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Events
{
	internal class EventHubService : TenantObject, IEventHubService
	{
		public EventHubService(ITenant tenant) : base(tenant)
		{
		}

		public async Task NotifyAsync(EventHubNotificationArgs e)
		{
			var args = new JObject
			{
				{"event", e.Name }
			};

			if (e.Arguments != null)
				args.Add("arguments", Serializer.Deserialize<JObject>(e.Arguments));

			if (EventHubs.Events != null)
			{
				var candidates = EventClients.Query(e.Name);

				if (candidates == null || candidates.Count == 0)
					return;

				var targets = new List<string>();

				var passedCandidates = AuthorizeCandidates(candidates, e.Arguments);

				foreach (var candidate in passedCandidates.Item1)
				{
					if (IsCandidateInterested(passedCandidates.Item2, candidate))
						targets.Add(candidate.ConnectionId);
				}

				if (targets.Count == 0)
					return;

				await EventHubs.Events.Clients.Clients(targets.AsReadOnly()).SendCoreAsync("event", new object[] { args });
			}
		}

		private bool IsCandidateInterested(IDistributedEventMiddleware middleware, EventClient client)
		{
			if (middleware == null)
				return true;

			middleware.Context.RevokeImpersonation();

			if (client.Arguments == null)
				return true;
			/*
			 * currently, only equal operator is supported.
			 */
			foreach(var item in client.Arguments.Properties())
			{
				if (!ResolveProperty(middleware, item))
					return false;
			}

			return true;
		}

		private bool ResolveProperty(object instance, JToken token)
		{
			if (token is not JProperty prop)
				return false;

			var property = instance.GetType().GetProperty(prop.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

			if (property is null)
				return false;

			if (prop.Value is JObject)
			{
				var newInstance = property.GetValue(instance);

				if (newInstance == null)
					return false;

				return ResolveProperty(newInstance, token);
			}
			else if (prop.Value is JValue value)
			{
				if (!Types.Compare(property.GetValue(instance), value.Value))
					return false;

				return true;
			}
			else
				return false;
		}

		private (List<EventClient>, IDistributedEventMiddleware) AuthorizeCandidates(ImmutableList<EventClient> candidates, string arguments)
		{
			if (candidates == null || candidates.Count == 0)
				return (candidates.ToList(), null);

			var result = new List<EventClient>();
			using var ctx = MicroServiceContext.FromIdentifier(candidates[0].EventName, Tenant);
			var instance = CreateMiddleware(ctx, candidates[0].EventName, arguments);

			if (instance == null)
				return (candidates.ToList(), null);

			foreach (var candidate in candidates)
			{
				instance.Context.Revoke();
				instance.Context.Impersonate(candidate.User.ToString());

				try
				{
					Authorize(instance, candidate.ConnectionId, null);
					result.Add(candidate);
				}
				catch (Exception ex)
				{
					instance.Context.Services.Diagnostic.Error(nameof(AuthorizeCandidates), ex.Message, LogCategories.Cdn);
					//authorization failed. nothing to do.
					continue;
				}
			}

			return (result, instance);
		}

		public void Authorize(string connectionId, string eventName, Guid user, object arguments)
		{
			using var ctx = MicroServiceContext.FromIdentifier(eventName, Tenant);

			ctx.Impersonate(user.ToString());

			var instance = CreateMiddleware(ctx, eventName, null);

			if (instance == null)
				return;

			object proxyInstance = null;

			if (arguments != null)
			{
				proxyInstance = CreateProxyInstance(instance);

				if (proxyInstance != null)
					Serializer.Populate(arguments, proxyInstance);
			}

			Authorize(instance, connectionId, proxyInstance);
		}

		private object CreateProxyInstance(IDistributedEventMiddleware middleware)
		{
			var type = middleware.GetType().FindAttribute<AuthorizationProxyAttribute>()?.Type;

			if(type == null)
				return null;

			return type.CreateInstance();
		}

		private IDistributedEventMiddleware CreateMiddleware(IMicroServiceContext context, string eventName, string arguments)
		{
			var descriptor = ComponentDescriptor.DistributedEvent(context, eventName);

			descriptor.Validate();

			if (descriptor.Configuration == null)
				throw new NotFoundException($"{SR.ErrCannotFindConfiguration} ({eventName})");

			var target = descriptor.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (target == null)
				throw new NotFoundException($"{SR.ErrDistributedEventNotFound} ({eventName})");

			var type = descriptor.Context.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, target, target.Name, false);

			if (type == null)
				return null;

			return descriptor.Context.CreateMiddleware<IDistributedEventMiddleware>(type, string.IsNullOrWhiteSpace(arguments) ? null : Serializer.Deserialize<JObject>(arguments));
		}

		private void Authorize(IDistributedEventMiddleware middleware, string connectionId, object proxyInstance)
		{
			middleware.Authorize(new EventConnectionArgs(connectionId, proxyInstance));
		}
	}
}
