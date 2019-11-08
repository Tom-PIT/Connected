using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;

namespace TomPIT.MicroServices.Search
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SearchCatalog : SourceCodeConfiguration, ISearchCatalogConfiguration
	{
	}
}
