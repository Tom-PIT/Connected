using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Security;

namespace TomPIT.MicroServices.Security
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class PermissionDescriptor : SourceCodeConfiguration, IPermissionDescriptorConfiguration
	{
	}
}
