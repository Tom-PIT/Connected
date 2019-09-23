using System;
using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Reporting
{
	[DomDesigner("TomPIT.Reporting.Design.Designers.ReportDesigner, TomPIT.Reporting.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomElement("TomPIT.Reporting.Design.Dom.ReportElement, TomPIT.Reporting.Design")]
	[ViewRenderer("TomPIT.Reporting.UI.ReportRenderer, TomPIT.Reporting")]
	public class Report : ComponentConfiguration, IReportConfiguration
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
