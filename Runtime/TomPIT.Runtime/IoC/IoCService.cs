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

		public T CreateMiddleware<T>() where T : class
		{
			return CreateMiddleware<T, object>(null);
		}

		public T CreateMiddleware<T, A>(A arguments) where T : class
		{
			var compiler = Tenant.GetService<ICompilerService>();
			var context = new MicroServiceContext(compiler.ResolveMicroService(typeof(T)));
			var instance = compiler.CreateInstance<T>(context, typeof(T));

			Serializer.Populate(arguments, instance);

			if (instance is IMiddlewareComponent component)
				component.Validate();

			return instance;
		}

		public List<IIoCEndpointMiddleware> CreateEndpoints<A>(IIoCContainerMiddleware sender, A e)
		{
			Initialize();

			var result = new List<IIoCEndpointMiddleware>();
			var name = sender.GetType().Name;

			if (!Endpoints.ContainsKey(name))
				return result;

			var endpoints = Endpoints[name];

			foreach (var endpoint in endpoints)
			{
				var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(sender.Context as IMicroServiceContext, endpoint.Type);

				if (endpointInstance == null)
					continue;

				Serializer.Populate(e, endpointInstance);

				if (!endpointInstance.CanHandleRequest())
					continue;

				result.Add(endpointInstance);
			}

			return result;
		}

		public bool HasEndpoints<A>(IIoCContainerMiddleware sender, A e)
		{
			Initialize();

			var result = new List<IIoCEndpointMiddleware>();
			var name = sender.GetType().Name;

			if (!Endpoints.ContainsKey(name))
				return false;

			var endpoints = Endpoints[name];

			foreach (var endpoint in endpoints)
			{
				var endpointInstance = Tenant.GetService<ICompilerService>().CreateInstance<IIoCEndpointMiddleware>(sender.Context as IMicroServiceContext, endpoint.Type);

				if (endpointInstance == null)
					continue;

				Serializer.Populate(e, endpointInstance);

				if (endpointInstance.CanHandleRequest())
					return true;
			}

			return false;
		}
		private ConcurrentDictionary<string, List<IoCEndpointDescriptor>> Endpoints => _endpoints.Value;
	}
}
