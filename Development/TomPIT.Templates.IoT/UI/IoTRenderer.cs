using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.UI;

namespace TomPIT.MicroServices.IoT.UI
{
	internal class IoTRenderer : IViewRenderer
	{
		public string CreateContent(ITenant connection, IComponent component)
		{
			return "@await Html.PartialAsync(\"~/Views/IoT/IoTView.cshtml\")";
		}
	}
}
