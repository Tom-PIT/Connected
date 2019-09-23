using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Reflection;
using TomPIT.UI;

namespace TomPIT.MicroServices.Reporting.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[EventArguments(typeof(ViewHelperArguments))]
	public class ViewHelper : Text, IViewHelper
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return GetType().ShortName();

			return Name;
		}
	}
}
