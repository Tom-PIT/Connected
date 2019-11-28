using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Deployment;

namespace TomPIT.MicroServices.Deployment
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Installer : SourceCodeConfiguration, IInstallerConfiguration
	{
	}
}
