using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	[ComponentCreatedHandler(DesignUtils.PartialCreateHandler)]
	public class Partial : ViewBase, IPartialViewConfiguration
	{
	}
}
