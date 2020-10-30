using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI.Theming
{
	[Create(DesignUtils.Less, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Less)]
	public class LessFile : ThemeFile, ILessFile
	{
	}
}
