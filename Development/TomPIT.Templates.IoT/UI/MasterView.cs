using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.IoT.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax("razor")]
	[ComponentCreatedHandler("TomPIT.Handlers.MasterCreateHandler, TomPIT.Development")]
	public class MasterView : ViewBase, IMasterView
	{
		public const string ComponentCategory = "MasterView";
	}
}
