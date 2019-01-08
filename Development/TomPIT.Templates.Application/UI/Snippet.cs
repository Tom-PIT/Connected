using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Snippet", nameof(Name))]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("razor")]
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
