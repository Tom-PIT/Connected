using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class ViewDependency : UIDependency, IViewDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.ViewsItems)]
		public string View { get; set; }
	}
}
