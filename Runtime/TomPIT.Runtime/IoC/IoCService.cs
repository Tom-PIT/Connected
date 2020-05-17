using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
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

				Type type = null;

				try
				{
					type = Tenant.GetService<ICompilerService>().ResolveType(microService, endpoint, endpoint.Name, false);
				}
				catch (Exception ex)
				{
					Tenant.LogError($"{ms.Name}/{endpoint.Configuration().ComponentName()}/{endpoint.Name}", ex.Message, LogCategories.IoC);
				}

				if (type == null)
					continue;

				var descriptor = new IoCEndpointDescriptor
				{
					Component = configuration.Component,
					Element = endpoint.Id,
					Type = type
				};

				if (Endpoints.ContainsKey(endpoint.Container))
				{
					var list = Endpoints[endpoint.Container];

					lock (list)
					{
						list.Add(descriptor);
					}
				}
				else
				{
					Endpoints.TryAdd(endpoint.Container, new List<IoCEndpointDescriptor>
					{
						descriptor
					});
				}
			}
		}

		protected override void OnRemoved(Guid microService, Guid component)
		{
			lock (_endpoints)
			{
				foreach (var endpoint in Endpoints)
				{
					var target = endpoint.Value.FirstOrDefault(f => f.Component == component);

					if (target != null)
					{
						endpoint.Value.Remove(target);
						break;
					}
				}
			}
		}

		public void Invoke(IMiddlewareContext context, IIoCOperation operation, object e = null)
		{
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCOperationMiddleware>(new MicroServiceContext(operation.Configuration().MicroService(), context), operation, Serializer.Serialize(e), operation.Name);

			if (instance is IIoCOperationContext iocContext)
				iocContext.Operation = operation;

			instance.Invoke();
		}

		public R Invoke<R>(IMiddlewareContext context, IIoCOperation operation, object e = null)
		{
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<object>(new MicroServiceContext(operation.Configuration().MicroService(), context), operation, Serializer.Serialize(e), operation.Name);

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

			foreach (var endpoint in endpoints)
			{
				var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(ctx, endpoint.Type, Serializer.Serialize(e));

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

			var ctx = new MicroServiceContext(operation.Configuration().MicroService(), context);

			foreach (var endpoint in endpoints)
			{
				var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(ctx, endpoint.Type, Serializer.Serialize(e));

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

			if (!Endpoints.ContainsKey(key))
				return null;

			return Endpoints[key];
		}
	}
}
