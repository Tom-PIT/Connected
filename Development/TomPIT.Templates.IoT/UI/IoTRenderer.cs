using TomPIT.ComponentModel.UI;
using TomPIT.Services;
using TomPIT.UI;

namespace TomPIT.IoT.UI
{
	internal class IoTRenderer : IViewRenderer
	{
		public string CreateContent(IExecutionContext context, IView view)
		{
			return "@await Html.PartialAsync(\"~/Views/IoT/IoTView.cshtml\")";
		}
	}
}
