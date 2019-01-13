using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.IoT
{
	public interface IIoTTransaction : IElement
	{
		string Name { get; }
		IServerEvent Invoke { get; }
	}
}
