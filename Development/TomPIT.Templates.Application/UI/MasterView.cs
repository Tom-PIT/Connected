using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	[ComponentCreatedHandler(DesignUtils.MasterCreateHandler)]
	public class MasterView : ViewBase, IMasterViewConfiguration
	{
	}
}
