using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;

namespace TomPIT.MicroServices.Navigation
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class SiteMap : SourceCodeConfiguration, ISiteMapConfiguration
	{
	}
}
