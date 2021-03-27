using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI.Theming
{
	[Create(DesignUtils.Stylesheet, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Css)]
	public class CssFile : ThemeFile, ICssFile
	{
		[Browsable(false)]
		public override string FileName => $"{ToString()}.css";
	}
}
