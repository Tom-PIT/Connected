namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class HslFunction : HslaFunction
	{
		protected override Node Evaluate(Env env)
		{
			Guard.ExpectNumArguments(3, Arguments.Count, this, Location);

			Arguments.Add(new Number(1d, ""));

			return base.Evaluate(env);
		}
	}
}