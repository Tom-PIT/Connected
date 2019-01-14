using TomPIT.ComponentModel;

namespace TomPIT.Workers
{
	public interface ICollectorProcess : IElement
	{
		ListItems<IProcessStep> Steps { get; }
	}
}
