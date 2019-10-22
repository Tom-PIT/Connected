using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
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
			foreach (var i in All())
				OnAdded(i.MicroService(), i.Component);
		}
		protected override void OnAdded(Guid microService, Guid component)
		{
			var configuration = Get(component);

			if (configuration == null || string.IsNullOrWhiteSpace(configuration.Container))
				return;

			var type = Tenant.GetService<ICompilerService>().ResolveType(microService, configuration, configuration.ComponentName(), false);

			if (type == null)
				return;

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			lock (_endpoints)
			{
				var descriptor = new IoCEndpointDescriptor
				{
					Component = configuration.Component,
					Type = type
				};

				if (Endpoints.ContainsKey(configuration.Container))
					Endpoints[configuration.Container].Add(descriptor);
				else
				{
					Endpoints.TryAdd(configuration.Container, new List<IoCEndpointDescriptor>
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

		public void Invoke(IIoCOperation operation)
		{
			Invoke(operation, null);
		}

		public void Invoke(IIoCOperation operation, object e)
		{
			var context = new MicroServiceContext(operation.Configuration().MicroService(), Tenant.Url);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCOperationMiddleware>(context, operation, Serializer.Serialize(e), operation.Name);

			if (instance is IIoCOperationContext iocContext)
				iocContext.Operation = operation;

			instance.Invoke();
		}

		public R Invoke<R>(IIoCOperation operation)
		{
			return Invoke<R>(operation, null);
		}

		public R Invoke<R>(IIoCOperation operation, object e)
		{
			var context = new MicroServiceContext(operation.Configuration().MicroService(), Tenant.Url);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<object>(context, operation, Serializer.Serialize(e), operation.Name);

			if (instance is IIoCOperationContext iocContext)
				iocContext.Operation = operation;

			var method = instance.GetType().GetMethod("Invoke");

			var r = method.Invoke(instance, null);

			return Marshall.Convert<R>(r);
		}

		public List<IIoCEndpointMiddleware> CreateEndpoints(IIoCOperation operation, object e)
		{
			Initialize();

			var result = new List<IIoCEndpointMiddleware>();
			var endpoints = ResolveEndpoints(operation);

			if (endpoints == null)
				return result;

			var ctx = new MicroServiceContext(operation.Configuration().MicroService(), Tenant.Url);

			foreach (var endpoint in endpoints)
			{
				var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(ctx, endpoint.Type);

				if (endpointInstance == null)
					continue;

				if (!endpointInstance.CanHandleRequest())
					continue;

				result.Add(endpointInstance);
			}

			return result;
		}

		public bool HasEndpoints(IIoCOperation operation, object e)
		{
			Initialize();

			var endpoints = ResolveEndpoints(operation);

			if (endpoints == null)
				return false;

			var ctx = new MicroServiceContext(operation.Configuration().MicroService(), Tenant.Url);

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
