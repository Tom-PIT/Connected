using TomPIT.ComponentModel.UI;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTView : IView
	{
		string Hub { get; set; }
	}
}
