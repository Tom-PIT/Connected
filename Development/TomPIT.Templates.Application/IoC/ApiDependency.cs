using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class ApiDependency : Dependency, IApiDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.ApiOperationListItems)]
		public string Operation { get; set; }
	}
}
