using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.MicroServices.Design;
using TomPIT.Middleware;

namespace TomPIT.MicroServices;

[Create(DesignUtils.Class)]
[DomDesigner(DomDesignerAttribute.TextDesigner)]
[Syntax(SyntaxAttribute.CSharp)]
[EventArguments(typeof(IMiddlewareContext))]
[FileNameExtension("cs")]
public class Code : TextConfiguration, ICodeConfiguration
{
	[Browsable(false)]
	public override string FileName => $"{ToString()}.cs";
}
