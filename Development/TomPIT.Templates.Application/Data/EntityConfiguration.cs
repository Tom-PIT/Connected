using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.MicroServices.Data;

[Syntax(SyntaxAttribute.CSharp)]
[DomDesigner(DomDesignerAttribute.TextDesigner)]
public class EntityConfiguration : TextConfiguration, IEntityConfiguration
{
	[Browsable(false)]
	public override string FileName => $"{ToString()}.csx";
}
