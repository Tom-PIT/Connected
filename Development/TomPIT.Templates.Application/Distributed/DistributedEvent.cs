using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.MicroServices.Distributed
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[Create("Event", nameof(Name))]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.DistributedEvent, TomPIT.MicroServices.Design")]
	[ClassRequired]
	public class DistributedEvent : SourceCodeElement, IDistributedEvent
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Designer | EnvironmentSection.Explorer)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
