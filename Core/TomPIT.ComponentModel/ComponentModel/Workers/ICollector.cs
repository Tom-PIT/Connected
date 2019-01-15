namespace TomPIT.ComponentModel.Workers
{
	public interface ICollector : IWorker
	{
		ListItems<ICollectorProcess> Processes { get; }
	}
}
