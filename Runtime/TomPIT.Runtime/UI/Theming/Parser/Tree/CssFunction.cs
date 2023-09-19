using System.Linq;
using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;

namespace TomPIT.UI.Theming.Parser.Tree
{
	public class CssFunction : Node
    {
        public string Name { get; set; }

        public Node Value { get; set; }

        protected override Node CloneCore() {
            return new CssFunction() {Name = Name, Value = Value.Clone()};
        }

        public override void AppendCSS(Env env)
        {
            env.Output.Append(string.Format("{0}({1})", Name, Value.ToCSS(env)));
        }

        public override Node Evaluate(Env env) {
            var node = (CssFunction)Clone();
            node.Value = Value.Evaluate(env);
            return node;
        }
    }

    public class CssFunctionList : NodeList
    {
        public override void AppendCSS(Env env)
        {
            env.Output.AppendMany(Inner, " ");
        }

        protected override Node CloneCore() {
            return new CssFunctionList();
        }

        public override Node Evaluate(Env env) {
            var node = (CssFunctionList) Clone();
            node.Inner = Inner.Select(i => i.Evaluate(env)).ToList();
            return node;
        }
    }
}
