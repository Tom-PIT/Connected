using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Configuration
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[Manifest(DesignUtils.SettingsManifest)]
	[ClassRequired]
	public class SettingsConfiguration : TextConfiguration, ISettingsConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }

		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
