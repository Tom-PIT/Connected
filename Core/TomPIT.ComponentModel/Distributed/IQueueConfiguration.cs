using TomPIT.Collections;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IQueueConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IQueueWorker> Workers { get; }
	}
}
