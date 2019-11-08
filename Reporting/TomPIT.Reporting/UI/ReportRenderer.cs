using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.UI;

namespace TomPIT.MicroServices.Reporting.UI
{
	internal class ReportRenderer : IViewRenderer
	{
		public string CreateContent(ITenant tenant, IComponent component)
		{
			var ms = tenant.GetService<IMicroServiceService>().Select(component.MicroService);
			var url = $"{ms.Name}/{component.Name}";

			return $"@await Html.PartialAsync(\"~/Views/Reporting/Report.cshtml\", new TomPIT.MicroServices.Reporting.Models.ReportRuntimeModel(Model, \"{url}\"))";
		}
	}
}
