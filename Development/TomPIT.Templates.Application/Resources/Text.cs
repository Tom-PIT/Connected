using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.MicroServices.Resources;

[FileNameExtension("txt")]
[DomDesigner(DomDesignerAttribute.TextDesigner)]
[Syntax(SyntaxAttribute.Text)]
public class Text : TextConfiguration, ITextConfiguration, IStaticFileConfiguration
{
	[Browsable(false)]
	public override string FileName => ToString();

	[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
	public string? Url { get; set; }
}
