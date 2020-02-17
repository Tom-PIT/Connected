using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;

namespace TomPIT.MicroServices.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SmtpConnection : SourceCodeConfiguration, ISmtpConnectionConfiguration
	{
	}
}
