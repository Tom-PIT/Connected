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
			var ms = Instance.Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)Worker).MicroService());
			var type = Instance.Tenant.GetService<ICompilerService>().ResolveType(ms.Token, Worker, Worker.ComponentName());
			var ctx = new MicroServiceContext(ms);
			var instance = Instance.Tenant.GetService<ICompilerService>().CreateInstance<IHostedWorkerMiddleware>(ctx, type);

			if (!string.IsNullOrWhiteSpace(State))
				SerializationExtensions.Populate(State, instance);

			instance.Invoke();

			State = SerializationExtensions.Serialize(instance);
		}
	}
}
