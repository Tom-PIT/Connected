namespace TomPIT.ComponentModel
{
	public interface IEventHandler : IConfiguration
	{
		IServerEvent Invoke { get; }

		ListItems<ITemplate> Scripts { get; }
		ListItems<IEventBinding> Events { get; }
	}
}
