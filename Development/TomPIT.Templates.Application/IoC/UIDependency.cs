using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.IoC;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.UiDependencyInjection, TomPIT.MicroServices.Design")]
	[Syntax(SyntaxAttribute.CSharp)]
	public abstract class UIDependency : Dependency, IUIDependency
	{
	}
}
