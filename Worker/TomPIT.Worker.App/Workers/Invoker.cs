namespace TomPIT.Worker.Workers
{
	public abstract class Invoker
	{
		public Invoker(string state)
		{
			State = state;
		}

		public abstract void Invoke();

		public string State { get; protected set; }
	}
}
