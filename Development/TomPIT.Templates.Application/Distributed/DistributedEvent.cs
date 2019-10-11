using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.MicroServices.Distributed
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class DistributedEvent : SourceCodeConfiguration, IDistributedEventConfiguration
	{

	}
}
