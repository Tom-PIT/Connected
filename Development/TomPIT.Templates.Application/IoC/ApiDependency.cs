using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.DependencyInjection, TomPIT.MicroServices.Design")]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class ApiDependency : Dependency, IApiDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.ApiOperationListItems)]
		public string Operation { get; set; }
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
