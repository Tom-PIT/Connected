using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class IoCEndpoint : SourceCodeElement, IIoCEndpoint
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[Required]
		[Items("TomPIT.MicroServices.Design.Items.IoCContainerItems, TomPIT.MicroServices.Design")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		public string Container { get; set; }

		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}
	}
}
