using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;
using TomPIT.UI.Theming.Parser.Tree;

namespace TomPIT.UI.Theming.Parser.Functions
{
	public class LengthFunction : ListFunctionBase
	{
		protected override Node Eval(Env env, Node[] list, Node[] args)
		{
			return new Number(list.Length);
		}
	}
}
