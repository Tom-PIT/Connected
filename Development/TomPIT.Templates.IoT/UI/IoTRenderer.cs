using TomPIT.ComponentModel.UI;
using TomPIT.Services;
using TomPIT.UI;

namespace TomPIT.IoT.UI
{
	internal class IoTRenderer : IViewRenderer
	{
		public string CreateContent()
		{
			return "@await Html.PartialAsync(\"~/Views/IoT/IoTView.cshtml\")";
		}
	}
}
