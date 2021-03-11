using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Worker.Workers
{
	internal class Hosted : Invoker
	{
		public Hosted(IHostedWorkerConfiguration worker, string state) : base(state)
		{
			Worker = worker;
		}

		private IHostedWorkerConfiguration Worker { get; }

		public override void Invoke()
		{
			var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)Worker).MicroService());
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ms.Token, Worker, Worker.ComponentName());
			using var ctx = new MicroServiceContext(ms);
			var instance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IHostedWorkerMiddleware>(ctx, type);

			if (!string.IsNullOrWhiteSpace(State))
				Serializer.Populate(State, instance);

			instance.Invoke();

			State = Serializer.Serialize(instance);
		}
	}
}
