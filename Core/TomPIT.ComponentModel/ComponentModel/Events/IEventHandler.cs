namespace TomPIT.ComponentModel.Events
{
	public interface IEventHandler : IConfiguration
	{
		IServerEvent Invoke { get; }

		ListItems<IText> Scripts { get; }
		ListItems<IEventBinding> Events { get; }
	}
}
