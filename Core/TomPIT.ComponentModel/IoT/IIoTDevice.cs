using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTDevice : IText, IElement
	{
		string Name { get; }
		string AuthenticationToken { get; }

		ListItems<IIoTTransaction> Transactions { get; }
	}
}
