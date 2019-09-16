using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Cdn
{
	[Create(DesignUtils.ComponentEvent, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SubscriptionEvent : SourceCodeElement, ISubscriptionEvent
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
		}
	}
}
