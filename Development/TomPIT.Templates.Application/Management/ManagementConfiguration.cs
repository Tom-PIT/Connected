using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Management;

namespace TomPIT.MicroServices.Management
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class ManagementConfiguration : SourceCodeConfiguration, IManagementConfiguration
	{
	}
}
