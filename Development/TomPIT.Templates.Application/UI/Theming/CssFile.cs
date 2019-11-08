using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.UI.Theming
{
	[Create(DesignUtils.Stylesheet, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Css)]
	public class CssFile : ThemeFile, ICssFile
	{
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.css", GetType().ShortName());

			return string.Format("{0}.css", Name);
		}

	}
}
