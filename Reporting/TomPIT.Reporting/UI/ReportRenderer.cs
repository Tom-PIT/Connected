using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;
using TomPIT.UI;

namespace TomPIT.Reporting.UI
{
	internal class ReportRenderer : IViewRenderer
	{
		public string CreateContent()
		{
			return "@await Html.PartialAsync(\"~/Views/Reporting/Report.cshtml\")";
		}
	}
}
