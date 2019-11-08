using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	public class Partial : ViewBase, IPartialViewConfiguration
	{
	}
}
