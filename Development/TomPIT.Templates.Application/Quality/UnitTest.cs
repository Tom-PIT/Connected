using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Quality;

namespace TomPIT.MicroServices.Quality
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class UnitTest : TextConfiguration, IUnitTestConfiguration
	{
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
