using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class SubscriptionEventDependency : Dependency, ISubscriptionEventDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.SubscriptionEventListItems)]
		public string Event { get; set; }
	}
}
