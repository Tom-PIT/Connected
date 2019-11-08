using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTHubConfiguration : IConfiguration
	{
		ListItems<IIoTDevice> Devices { get; }
		string Schema { get; }
		ElementScope Scope { get; }
	}
}
