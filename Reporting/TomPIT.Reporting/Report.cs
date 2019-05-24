using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;

namespace TomPIT.Reporting
{
	[DomDesigner("TomPIT.Reporting.Design.Designers.ReportDesigner, TomPIT.Reporting.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	[DomElement("TomPIT.Reporting.Design.Dom.ReportElement, TomPIT.Reporting.Design")]
	[ViewRenderer("TomPIT.Reporting.UI.ReportRenderer, TomPIT.Reporting")]
	public class Report : ComponentConfiguration, IReport
	{
	}
}
