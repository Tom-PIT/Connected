using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Runtime;

namespace TomPIT.MicroServices.Runtime
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class RuntimeConfiguration : SourceCodeConfiguration, IRuntimeConfiguration
	{
	}
}
