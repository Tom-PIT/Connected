using TomPIT.ComponentModel.Workers;

namespace TomPIT.Worker.Workers
{
	public class Collector : Invoker
	{
		public Collector(WorkerInvokeArgs e, ICollector worker) : base(e)
		{
			Worker = worker;
		}

		private ICollector Worker { get; }

		public override void Invoke()
		{

		}
	}
}
