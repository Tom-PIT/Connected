using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;

namespace TomPIT.MicroServices.Configuration
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.MicroServiceInfo, TomPIT.MicroServices.Design")]
	public class MicroServiceInfoConfiguration : SourceCodeConfiguration, IMicroServiceInfoConfiguration
	{
	}
}
