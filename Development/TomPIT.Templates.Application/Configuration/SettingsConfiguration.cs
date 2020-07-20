using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;

namespace TomPIT.MicroServices.Configuration
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SettingsConfiguration : SourceCodeConfiguration, ISettingsConfiguration
	{
	}
}
