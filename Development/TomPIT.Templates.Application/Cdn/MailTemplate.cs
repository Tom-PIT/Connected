using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;

namespace TomPIT.MicroServices.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	public class MailTemplate : SourceCodeConfiguration, IMailTemplateConfiguration
	{
	}
}
