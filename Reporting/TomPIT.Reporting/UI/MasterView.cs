using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Reporting.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax("razor")]
	[ComponentCreatedHandler("TomPIT.Handlers.MasterCreateHandler, TomPIT.Development")]
	public class MasterView : ViewBase, IMasterView
	{
		public const string ComponentCategory = "MasterView";
	}
}
