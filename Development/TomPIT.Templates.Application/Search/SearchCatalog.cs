using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;

namespace TomPIT.MicroServices.Search
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class SearchCatalog : SourceCodeConfiguration, ISearchCatalogConfiguration
	{
	}
}
