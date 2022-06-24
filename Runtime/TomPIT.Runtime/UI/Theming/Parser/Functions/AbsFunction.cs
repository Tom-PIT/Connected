namespace TomPIT.UI.Theming.Parser.Functions
{
	using System;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;

	public class AbsFunction : NumberFunctionBase
	{
		protected override Node Eval(Env env, Number number, Node[] args)
		{
			WarnNotSupportedByLessJS("abs(number)");

			return new Number(Math.Abs(number.Value), number.Unit);
		}
	}
}