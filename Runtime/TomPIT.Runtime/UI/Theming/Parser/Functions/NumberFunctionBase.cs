namespace TomPIT.UI.Theming.Parser.Functions
{
	using System.Linq;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public abstract class NumberFunctionBase : Function
    {
        protected override Node Evaluate(Env env)
        {
            Guard.ExpectMinArguments(1, Arguments.Count, this, Location);
            Guard.ExpectNode<Number>(Arguments[0], this, Arguments[0].Location);

            var number = Arguments[0] as Number;
            var args = Arguments.Skip(1).ToArray();

            return Eval(env, number, args);
        }

        protected abstract Node Eval(Env env, Number number, Node[] args);
    }
}