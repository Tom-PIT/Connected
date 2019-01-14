using TomPIT.ComponentModel;

namespace TomPIT.Workers
{
	public interface ICollector : IWorker
	{
		ListItems<ICollectorProcess> Processes { get; }
	}
}
