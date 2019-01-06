using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application.UI
{
	[Create("Snippet", nameof(Name))]
	[DomDesigner("TomPIT.Design.TemplateDesigner, TomPIT.Ide")]
	[Syntax("razor")]
	public class Snippet : Text
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
