using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Runtime;

namespace TomPIT.MicroServices.Runtime
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class RuntimeConfiguration : SourceCodeConfiguration, IRuntimeConfiguration
	{
	}
}
