using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Scripting
{
	[Create(DesignUtils.Javascript, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Javascript)]
	public class Javascript : Text
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		[Browsable(false)]
		public override string FileName => $"{ToString()}.js";
	}
}
