using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.UI;

namespace TomPIT.Reporting.UI
{
	internal class ReportRenderer : IViewRenderer
	{
		public string CreateContent(ISysConnection connection, IComponent component)
		{
			var ms = connection.GetService<IMicroServiceService>().Select(component.MicroService);
			var url = $"{ms.Name}/{component.Name}";

			return $"@await Html.PartialAsync(\"~/Views/Reporting/Report.cshtml\", new TomPIT.Reporting.Models.ReportRuntimeModel(Model, \"{url}\"))";
		}
	}
}
