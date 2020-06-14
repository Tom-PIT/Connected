using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.MicroServices.IoT
{
	[Syntax(SyntaxAttribute.CSharp)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	public class Hub : SourceCodeConfiguration, IIoTHubConfiguration
	{
		public ElementScope Scope { get; set; } = ElementScope.Internal;
	}
}
