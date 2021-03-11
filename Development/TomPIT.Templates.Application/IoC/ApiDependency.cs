using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class ApiDependency : Dependency, IApiDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.ApiOperationListItems)]
		public string Operation { get; set; }
	}
}
