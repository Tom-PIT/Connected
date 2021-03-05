using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	[ComponentCreatedHandler(DesignUtils.MasterCreateHandler)]
	public class MasterView : ViewBase, IMasterViewConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Inherits { get; set; }
	}
}
