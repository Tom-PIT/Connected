using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Services;

namespace TomPIT.IoC
{
	internal class IoCService : ConfigurationRepository<IIoCEndpointConfiguration>, IIoCService
	{
		private Lazy<ConcurrentDictionary<string, List<IoCEndpointDescriptor>>> _endpoints = new Lazy<ConcurrentDictionary<string, List<IoCEndpointDescriptor>>>();
		public IoCService(ITenant tenant) : base(tenant, "iocendpoint")
		{
			Tenant.GetService<ICompilerService>().Invalidated += OnInvalidateScript;
		}

		private void OnInvalidateScript(object sender, Guid e)
		{
			var obsolete = new List<IoCEndpointDescriptor>();

			foreach (var container in Endpoints)
			{
				lock (container.Value)
				{
					foreach (var endpoint in container.Value)
					{
						/*
						 * It's possible the endpoint couldn't compile and it would mean
						 * it's lost because it's type is null and initialized property set
						 * to true. It would never get initialized again, except when 
						 * touching script directly so it's the best we remove it in any case.
						 */
						if (endpoint.Type is null)
						{
							obsolete.Add(endpoint);
							continue;
						}

						if (CompilerExtensions.HasScriptReference(endpoint.Type.Assembly, e))
							obsolete.Add(endpoint);
					}
				}
			}

			foreach (var endpoint in obsolete)
				OnChanged(endpoint.MicroService, endpoint.Component);
		}

		protected override string[] Categories => new string[] { ComponentCategories.IoCEndpoint };

		protected override void OnChanged(Guid microService, Guid component)
		{
			OnRemoved(microService, component);
			OnAdded(microService, component);
		}

		protected override void OnInitialized()
		{
			Parallel.ForEach(All(), (i) =>
			{
				OnAdded(i.MicroService(), i.Component);
			});
		}
		protected override void OnAdded(Guid microService, Guid component)
		{
			var configuration = Get(component);

			if (configuration == null)
				return;

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			foreach (var endpoint in configuration.Endpoints)
			{
				if (string.IsNullOrWhiteSpace(endpoint.Name) || string.IsNullOrWhiteSpace(endpoint.Container))
					continue;

				var descriptor = new IoCEndpointDescriptor(Tenant)
				{
					Endpoint = endpoint,
					Component = configuration.Component,
					MicroService = ms.Token
				};

				if (Endpoints.TryGetValue(endpoint.Container, out List<IoCEndpointDescriptor> list))
				{
					lock (list)
						list.Add(descriptor);
				}
				else
				{
					lock (Endpoints)
						if (!Endpoints.TryAdd(endpoint.Container, new List<IoCEndpointDescriptor>
						{
							descriptor
						}))
						{
							if (!Endpoints.TryGetValue(endpoint.Container, out List<IoCEndpointDescriptor> items))
								Tenant.LogWarning(nameof(IoCService), $"Cannot register endpoint {configuration.ComponentName()}");
							else
							{
								lock (items)
									items.Add(descriptor);
							}
						}
				}
			}
		}

		protected override void OnRemoved(Guid microService, Guid component)
		{
			lock (Endpoints)
				foreach (var endpoint in Endpoints)
				{
					var targets = endpoint.Value.Where(f => f.Component == component).ToImmutableArray();

					if (targets.Any())
					{
						lock (endpoint.Value)
						{
							foreach (var target in targets)
								endpoint.Value.Remove(target);
						}
					}
				}
		}

		public void Invoke(IMiddlewareContext context, IIoCOperation operation, object e = null)
		{
			using var ctx = new MicroServiceContext(operation.Configuration().MicroService(), context);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCOperationMiddleware>(ctx, operation, e, operation.Name);

			if (instance is IIoCOperationContext iocContext)
				iocContext.Operation = operation;

			instance.Invoke();
		}

		public R Invoke<R>(IMiddlewareContext context, IIoCOperation operation, object e = null)
		{
			using var ctx = new MicroServiceContext(operation.Configuration().MicroService(), context);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<object>(ctx, operation, e, operation.Name);

			if (instance is IIoCOperationContext iocContext)
				iocContext.Operation = operation;

			var method = instance.GetType().GetMethod("Invoke");

			var r = method.Invoke(instance, null);

			return Marshall.Convert<R>(r);
		}

		public List<IIoCEndpointMiddleware> CreateEndpoints(IMiddlewareContext context, IIoCOperation operation, object e)
		{
			Initialize();

			var result = new List<IIoCEndpointMiddleware>();
			var endpoints = ResolveEndpoints(operation);

			if (endpoints == null)
				return result;

			var ctx = new MicroServiceContext(operation.Configuration().MicroService(), context);

			lock (endpoints)
				foreach (var endpoint in endpoints)
				{
					if (endpoint.Type == null)
						continue;

					var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(ctx, endpoint.Type, e);

					if (endpointInstance == null)
						continue;

					if (!endpointInstance.CanHandleRequest())
						continue;

					result.Add(endpointInstance);
				}

			return result;
		}

		public bool HasEndpoints(IMiddlewareContext context, IIoCOperation operation, object e)
		{
			Initialize();

			var endpoints = ResolveEndpoints(operation);

			if (endpoints == null)
				return false;

			using var ctx = new MicroServiceContext(operation.Configuration().MicroService(), context);

			lock (endpoints)
				foreach (var endpoint in endpoints)
				{
					if (endpoint.Type == null)
						continue;

					var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(ctx, endpoint.Type, e);

					if (endpointInstance == null)
						continue;

					if (endpointInstance.CanHandleRequest())
						return true;
				}

			return false;
		}
		private ConcurrentDictionary<string, List<IoCEndpointDescriptor>> Endpoints => _endpoints.Value;

		private List<IoCEndpointDescriptor> ResolveEndpoints(IIoCOperation operation)
		{
			if (string.IsNullOrWhiteSpace(operation.Name))
				return null;

			var name = operation.Name;
			var ms = Tenant.GetService<IMicroServiceService>().Select(operation.Configuration().MicroService());
			var component = Tenant.GetService<IComponentService>().SelectComponent(operation.Configuration().Component);

			if (ms == null || component == null)
				return null;

			var key = $"{ms.Name}/{component.Name}/{name}";

			if (Endpoints.TryGetValue(key, out List<IoCEndpointDescriptor> result))
				return result;

			return null;
		}
	}
}
