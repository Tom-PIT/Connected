using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;

namespace TomPIT.MicroServices.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class SmtpConnection : TextConfiguration, ISmtpConnectionConfiguration
	{
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
