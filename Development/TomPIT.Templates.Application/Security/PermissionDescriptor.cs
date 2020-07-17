using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Security;

namespace TomPIT.MicroServices.Security
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class PermissionDescriptor : SourceCodeConfiguration, IPermissionDescriptorConfiguration
	{
	}
}
