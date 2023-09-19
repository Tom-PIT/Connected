using System.Collections.Generic;
using System.Linq;
using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;

namespace TomPIT.UI.Theming.Parser.Tree
{
	public class Extend : Node
    {
        public Extend(List<Selector> exact, List<Selector> partial)
        {
            Exact = exact;
            Partial = partial;
        }

        public List<Selector> Exact{ get; set; }
        public List<Selector> Partial { get; set; }


        public override Node Evaluate(Env env)
        {
            var newExact = new List<Selector>();
            foreach (var e in Exact)
            {
                var childContext = env.CreateChildEnv();
                e.AppendCSS(childContext);
                var selector = new Selector(new []{new Element(e.Elements.First().Combinator,childContext.Output.ToString().Trim())});
                selector.IsReference = IsReference;
                newExact.Add(selector);
            }

            var newPartial = new List<Selector>();
            foreach (var e in Partial)
            {
                var childContext = env.CreateChildEnv();
                e.AppendCSS(childContext);
                var selector = new Selector(new[] { new Element(e.Elements.First().Combinator, childContext.Output.ToString().Trim()) });
                selector.IsReference = IsReference;
                newPartial.Add(selector);
            }

            return new Extend(newExact,newPartial) { IsReference = IsReference, Location = Location };
        }

        protected override Node CloneCore() {
            return new Extend(Exact.Select(e => (Selector)e.Clone()).ToList(), Partial.Select(e => (Selector)e.Clone()).ToList());
        }

        public override void AppendCSS(Env env)
        {
            
        }

        public override bool IgnoreOutput() {
            return true;
        }
    }
}
