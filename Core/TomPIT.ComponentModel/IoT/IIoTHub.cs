using TomPIT.ComponentModel;

namespace TomPIT.IoT
{
	public interface IIoTHub : IConfiguration
	{
		ListItems<IIoTDevice> Devices { get; }
		ListItems<IIoTSchemaField> Schema { get; }
	}
}
