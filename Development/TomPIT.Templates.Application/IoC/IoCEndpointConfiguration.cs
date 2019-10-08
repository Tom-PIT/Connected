using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class IoCEndpointConfiguration : SourceCodeConfiguration, IIoCEndpointConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[Required]
		[Items("TomPIT.MicroServices.Design.Items.IoCContainerItems, TomPIT.MicroServices.Design")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		public string Container { get; set; }
	}
}
