using System;
using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Reporting
{
	[DomDesigner("TomPIT.MicroServices.Reporting.Design.Designers.ReportDesigner, TomPIT.MicroServices.Reporting.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomElement("TomPIT.MicroServices.Reporting.Design.Dom.ReportElement, TomPIT.MicroServices.Reporting.Design")]
	[ViewRenderer("TomPIT.MicroServices.Reporting.UI.ReportRenderer, TomPIT.MicroServices.Reporting")]
	public class Report : ComponentConfiguration, IReportConfiguration
	{
		private List<string> _apis = null;

		[Browsable(false)]
		public Guid TextBlob { get; set; }

		public List<string> Apis
		{
			get
			{
				if (_apis == null)
					_apis = new List<string>();

				return _apis;
			}
		}

		[Browsable(false)]
		public string FileName => $"{ToString()}.csx";
	}
}
