using TomPIT.ComponentModel;

namespace TomPIT.IoT
{
	public class Hub : ComponentConfiguration
	{
		ListItems<HubDevice> Devices { get; }
	}
}
