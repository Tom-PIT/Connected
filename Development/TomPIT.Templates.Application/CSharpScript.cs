using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
	[Create("Class", nameof(Name))]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("csharp")]
	public class CSharpScript : Text, ISourceCode
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.csx", GetType().ShortName());

			return string.Format("{0}.csx", Name);
		}
	}
}
