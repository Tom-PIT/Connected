using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.IoT
{
	public enum IoTTransactonScope
	{
		Global = 1,
		Scoped = 2
	}
	public interface IIoTTransaction : IConfigurationElement
	{
		string Name { get; }
		IServerEvent Invoke { get; }
		IoTTransactonScope Scope { get; }
	}
}
