namespace TomPIT.UI.Theming.Parser.Infrastructure
{
	using System.Collections.Generic;
	using TomPIT.UI.Theming.Parser.Tree;

	public class Closure
    {
        public Ruleset Ruleset { get; set; }
        public List<Ruleset> Context { get; set; }
    }
}