using TomPIT.ComponentModel.UI;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTViewConfiguration : IViewConfiguration
	{
		string Hub { get; set; }
	}
}
