using TomPIT.ComponentModel.Workers;

namespace TomPIT.Worker.Workers
{
	public abstract class Invoker
	{
		public Invoker(WorkerInvokeArgs e)
		{
			Args = e;
		}

		public abstract void Invoke();

		protected WorkerInvokeArgs Args { get; }
	}
}
