using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Data;

namespace TomPIT.MicroServices.Data
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Sql)]
	public class DatabaseOperation : ModelOperation, IDatabaseOperation
	{
	}
}
