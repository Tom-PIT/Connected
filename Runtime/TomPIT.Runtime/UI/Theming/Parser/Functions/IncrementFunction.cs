namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;

	public class IncrementFunction : NumberFunctionBase
	{
		protected override Node Eval(Env env, Number number, Node[] args)
		{
			return new Number(number.Value + 1, number.Unit);
		}
	}
}