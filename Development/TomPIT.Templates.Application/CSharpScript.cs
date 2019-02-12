using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Application
{
	[Create("Class", nameof(Name))]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("csharp")]
	[EventArguments(typeof(IExecutionContext))]
	public class CSharpScript : Text, IPartialSourceCode
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
