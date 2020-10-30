using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Quality;

namespace TomPIT.MicroServices.Quality
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class UnitTest : SourceCodeConfiguration, IUnitTestConfiguration
	{
	}
}
