namespace TomPIT.UI.Theming.Parser.Functions
{
	using System.Linq;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class HslaFunction : Function
    {
        protected override Node Evaluate(Env env)
        {
            Guard.ExpectNumArguments(4, Arguments.Count, this, Location);
            Guard.ExpectAllNodes<Number>(Arguments, this, Location);

            var args = Arguments.Cast<Number>().ToArray();

            return new ThemeHslColor(args[0], args[1], args[2], args[3]).ToRgbColor();
        }
    }
}