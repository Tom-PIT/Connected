using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.UI;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[DomElement(DesignUtils.ViewElement)]
	[Syntax(SyntaxAttribute.Razor)]
	[ComponentCreatedHandler(DesignUtils.ViewCreateHandler)]
	public class View : ViewBase, IViewConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Layout { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(true)]
		public bool Enabled { get; set; } = true;

		[DefaultValue(true)]
		public bool AuthorizationEnabled { get; set; } = true;
	}
}