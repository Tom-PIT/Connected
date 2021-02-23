using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;

namespace TomPIT.MicroServices.Configuration
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.MicroServiceInfo, TomPIT.MicroServices.Design")]
	[ClassRequired]
	public class MicroServiceInfoConfiguration : SourceCodeConfiguration, IMicroServiceInfoConfiguration
	{
	}
}
