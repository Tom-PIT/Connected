using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;

namespace TomPIT.Reporting.Design.Designers
{
	internal class ReportDesigner : DomDesigner<IDomElement>
	{
		public ReportDesigner(IDomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Report.cshtml";
		public override object ViewModel => this;
	}
}
