using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Workers;
using TomPIT.Data;
using TomPIT.Services;
using TomPIT.Workers;

namespace TomPIT.Worker.Workers
{
	internal class Hosted : Invoker
	{
		public Hosted(IHostedWorker worker, string state) : base(state)
		{
			Worker = worker;
		}

		private IHostedWorker Worker { get; }

		public override void Invoke()
		{
			var ms = Instance.Connection.GetService<IMicroServiceService>().Select(((IConfiguration)Worker).MicroService(Instance.Connection));
			var type = Instance.GetService<ICompilerService>().ResolveType(ms.Token, Worker, Worker.ComponentName(Instance.Connection));
			var ctx = ExecutionContext.Create(Instance.Connection.Url, ms);
			var dataCtx = new DataModelContext(ctx);
			var instance = type.CreateInstance<IHostedWorkerHandler>(new object[] { dataCtx });

			if (!string.IsNullOrWhiteSpace(State))
				Types.Populate(State, instance);

			instance.Invoke();

			State = Types.Serialize(instance);
		}
	}
}
