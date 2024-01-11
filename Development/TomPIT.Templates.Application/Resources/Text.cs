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
	public override string FileName
	{
		get
		{
			var preferred = ToString();

			if (preferred.Contains('.'))
				return preferred;

			return $"{preferred}.{Extension}";
		}
	}

	[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
	public string? Extension { get; set; } = "txt";

	[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
	public string? Url { get; set; }
}
