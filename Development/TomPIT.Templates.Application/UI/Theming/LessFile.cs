using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.UI.Theming
{
	[Create(DesignUtils.Less, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Less)]
	public class LessFile : ThemeFile, ILessFile
	{
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.less", GetType().ShortName());

			return string.Format("{0}.less", Name);
		}
	}
}
