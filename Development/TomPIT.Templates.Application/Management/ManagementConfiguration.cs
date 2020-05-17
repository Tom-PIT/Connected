using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Management;

namespace TomPIT.MicroServices.Management
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class ManagementConfiguration : SourceCodeConfiguration, IManagementConfiguration
	{
	}
}
