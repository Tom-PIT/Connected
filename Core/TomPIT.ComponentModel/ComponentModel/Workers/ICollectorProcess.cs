namespace TomPIT.ComponentModel.Workers
{
	public interface ICollectorProcess : IElement
	{
		ListItems<IProcessStep> Steps { get; }
	}
}
