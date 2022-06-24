using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;

namespace TomPIT.UI.Theming.Parser.Functions
{
	public class DefaultFunction : Function
    {
        protected override Node Evaluate(Env env)
        {
            return new TextNode("default()");
        }
    }
}
