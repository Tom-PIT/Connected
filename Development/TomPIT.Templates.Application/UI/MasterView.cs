using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	[ComponentCreatedHandler("TomPIT.Handlers.MasterCreateHandler, TomPIT.Development")]
	public class MasterView : ViewBase, IMasterView
	{
		public const string ComponentCategory = "MasterView";
	}
}
