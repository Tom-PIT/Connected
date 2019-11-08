using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;

namespace TomPIT.MicroServices.Navigation
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SiteMap : SourceCodeConfiguration, ISiteMapConfiguration
	{
	}
}
