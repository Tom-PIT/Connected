using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;
using TomPIT.UI.Theming.Parser.Tree;
using TomPIT.UI.Theming.Utils;

namespace TomPIT.UI.Theming.Parser.Functions
{
	public class ExtractFunction : ListFunctionBase
    {
        protected override Node Eval(Env env, Node[] list, Node[] args)
        {
            Guard.ExpectNumArguments(1, args.Length, this, Location);
            Guard.ExpectNode<Number>(args[0], this, args[0].Location);

            var index = (int)(args[0] as Number).Value;

            // Extract function indecies are 1-based
            return list[index-1];
        }
    }
}
