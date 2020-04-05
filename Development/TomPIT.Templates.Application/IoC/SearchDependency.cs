using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SearchDependency : Dependency, ISearchDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.SearchCatalogListItems)]
		public string Catalog { get; set; }
	}
}
