using TomPIT.UI.Theming.Parser.Tree;

namespace TomPIT.UI.Theming.Parser.Infrastructure
{
	public class NamedArgument
	{
		public string Name { get; set; }
		public Expression Value { get; set; }
	}
}