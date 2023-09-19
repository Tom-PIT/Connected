using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;
using TomPIT.UI.Theming.Plugins;

namespace TomPIT.UI.Theming.Parser.Tree
{
	public class GuardedRuleset : Ruleset
    {
        public GuardedRuleset(NodeList<Selector> selectors, NodeList rules, Condition condition)
            : base(selectors, rules)
        {
            this.Condition = condition;
        }

        public Condition Condition { get; set; }

        public override Node Evaluate(Env env)
        {
            if (Condition.Passes(env))
            {
                return EvaluateRulesForFrame(this, env);
            }
            return new NodeList();
        }

        public override void Accept(IVisitor visitor)
        {
            base.Accept(visitor);
            Condition = VisitAndReplace(Condition, visitor);
        }

        public override void AppendCSS(Env env)
        {
        }
    }
}
