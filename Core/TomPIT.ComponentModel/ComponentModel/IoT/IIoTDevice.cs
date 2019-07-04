using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTDevice : ISourceCode
	{
		string Name { get; }
		string AuthenticationToken { get; }

		ListItems<IIoTTransaction> Transactions { get; }
	}
}
