using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.IoT
{
	public interface IIoTDevice : IElement
	{
		string Name { get; }
		string AuthenticationToken { get; }
		IServerEvent Data { get; }

		ListItems<IIoTTransaction> Transactions { get; }
	}
}
