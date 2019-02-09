using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTTransaction : IConfigurationElement
	{
		string Name { get; }
		IServerEvent Invoke { get; }
		ListItems<IIoTTransactionParameter> Parameters { get; }
	}
}
