using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.UI;

namespace TomPIT.IoT.UI
{
	internal class IoTRenderer : IViewRenderer
	{
		public string CreateContent(ISysConnection connection, IComponent component)
		{
			return "@await Html.PartialAsync(\"~/Views/IoT/IoTView.cshtml\")";
		}
	}
}
