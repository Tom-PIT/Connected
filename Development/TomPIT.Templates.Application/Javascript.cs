using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
	[Create("Javascript", nameof(Name))]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("javascript")]
	public class Javascript : Text
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.js", GetType().ShortName());

			return string.Format("{0}.js", Name);
		}
	}
}
