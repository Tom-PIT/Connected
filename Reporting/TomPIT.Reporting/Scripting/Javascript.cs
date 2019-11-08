using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Reporting.Scripting
{
	[Create("Javascript", nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
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
