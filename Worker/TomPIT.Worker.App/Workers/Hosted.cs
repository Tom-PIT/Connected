using TomPIT.Compilation;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Worker.Workers
{
	internal class Hosted : Invoker
	{
		public Hosted(WorkerInvokeArgs e, IHostedWorker worker) : base(e)
		{
			Worker = worker;
		}

		private IHostedWorker Worker { get; }

		public override void Invoke()
		{
			Instance.GetService<ICompilerService>().Execute(Worker.MicroService(Instance.Connection), Worker.Invoke, this, Args);
		}
	}
}
