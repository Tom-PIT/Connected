namespace TomPIT.UI.Theming.Parser.Functions
{
	using System.Linq;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class AddFunction : Function
	{
		protected override Node Evaluate(Env env)
		{
			Guard.ExpectAllNodes<Number>(Arguments, this, Location);

			var value = Arguments.Cast<Number>().Select(d => d.Value).Aggregate(0d, (a, b) => a + b);

			return new Number(value);
		}
	}
}