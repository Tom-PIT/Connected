using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.MicroServices.IoT.Design;

namespace TomPIT.MicroServices.IoT
{
	[Syntax(SyntaxAttribute.CSharp)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[ComponentCreatedHandler(DesignUtils.IoTHubCreateHandler)]
	[ClassRequired]
	public class Hub : SourceCodeConfiguration, IIoTHubConfiguration
	{
		public ElementScope Scope { get; set; } = ElementScope.Internal;
	}
}
