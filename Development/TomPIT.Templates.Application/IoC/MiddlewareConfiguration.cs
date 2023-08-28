using System;
using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;

namespace TomPIT.MicroServices.IoC;

[DomDesigner(DomDesignerAttribute.TextDesigner)]
[Syntax(SyntaxAttribute.CSharp)]
public class MiddlewareConfiguration : ComponentConfiguration, IMiddlewareConfiguration
{
    [Browsable(false)]
    public Guid TextBlob { get; set; }
    [Browsable(false)]
    public string FileName => $"{ToString()}.csx";
}
