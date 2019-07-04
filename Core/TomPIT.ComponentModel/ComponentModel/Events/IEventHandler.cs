namespace TomPIT.ComponentModel.Events
{
	public interface IEventHandler : IConfiguration, ISourceCode
	{
		ListItems<IEventBinding> Events { get; }
	}
}
