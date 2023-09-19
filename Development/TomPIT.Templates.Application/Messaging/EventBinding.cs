using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Messaging
{
	[Create(DesignUtils.EventBinding)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.EventBinding, TomPIT.MicroServices.Design")]
	[ClassRequired]
	public class EventBinding : TextElement, IEventBinding
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.EventItems)]
		public string Event { get; set; }

		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? GetType().ShortName()
				: Name;
		}
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
