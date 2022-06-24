namespace TomPIT.UI.Theming.Parser.Functions
{
	using System;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;

	public class CeilFunction : NumberFunctionBase
    {
        protected override Node Eval(Env env, Number number, Node[] args)
        {
            return new Number(Math.Ceiling(number.Value), number.Unit);
        }
    }
}