using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Scripting
{
	[Create(DesignUtils.Javascript, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Javascript)]
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
