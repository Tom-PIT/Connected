using System;
using System.Reflection;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Cdn.Data
{
	internal class EndpointConnectionDescriptor : TenantObject
	{
		public EndpointConnectionDescriptor(ITenant tenant, IDataHubEndpointPolicy policy, string arguments) : base(tenant)
		{
			Policy = policy;
			Component = policy.Configuration().Component;
			Arguments = arguments;
		}

		public IDataHubEndpointPolicy Policy { get; set; }
		private string Arguments { get; set; }
		public Guid Component { get; }

		public void Notify(string connectionId, string arguments)
		{
			var context = new MicroServiceContext(Policy.Configuration().MicroService(), Tenant.Url);
			var instance = CreateInstance(context);
			var invoker = CreateInvoker(instance);
			var middleware = instance.GetType().GetInterface(typeof(IDataHubEndpointPolicyMiddleware<>).FullName);
			var argumentType = middleware.GetGenericArguments()[0];
			var invokeArgument = Tenant.GetService<ICompilerService>().CreateInstance<object>(context, argumentType, arguments);

			if (Types.Convert<bool>(invoker.Invoke(instance, new object[] { invokeArgument })))
				DataHubs.Data.Clients.Client(connectionId).SendCoreAsync("data", new object[] { Endpoint, Policy.Name, instance }).Wait();
		}

		private string Endpoint => Policy.Closest<IDataHubEndpoint>().Name;
		private MethodInfo CreateInvoker(object instance)
		{
			return instance.GetType().GetMethod(nameof(IDataHubEndpointPolicyMiddleware<object>.Invoke));
		}

		private object CreateInstance(IMicroServiceContext context)
		{
			return Tenant.GetService<ICompilerService>().CreateInstance<object>(context, Policy, Arguments, Policy.Name);
		}
	}
}