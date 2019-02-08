using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.IoT.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	public class Snippet : Text, ISnippet
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
