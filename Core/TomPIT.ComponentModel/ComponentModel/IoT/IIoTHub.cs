namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTHub : IConfiguration
	{
		ListItems<IIoTDevice> Devices { get; }
		string Schema { get; }
		ElementScope Scope { get; }
	}
}
