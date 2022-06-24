﻿namespace TomPIT.UI.Theming.Parser.Tree
{
	using System.Linq;
	using Infrastructure;
	using Infrastructure.Nodes;

	public class Directive : Ruleset
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public Node Value { get; set; }

        public Directive(string name, string identifier, NodeList rules)
        {
            Name = name;
            Rules = rules;
            Identifier = identifier;
        }

        public Directive(string name, Node value)
        {
            Name = name;
            Value = value;
        }

        protected Directive()
        {
        }

        protected override Node CloneCore() {
            if (Rules != null) {
                return new Directive(Name, Identifier, (NodeList) Rules.Clone());
            }
            return new Directive(Name, Value.Clone());
        }

        public override void Accept(Plugins.IVisitor visitor)
        {
            Rules = VisitAndReplace(Rules, visitor);
            Value = VisitAndReplace(Value, visitor);
        }

        public override Node Evaluate(Env env)
        {
            env.Frames.Push(this);

            Directive evaldDirective;

            if (Rules != null)
            {
                evaldDirective = new Directive(Name, Identifier, new NodeList(Rules.Select(r => r.Evaluate(env))).ReducedFrom<NodeList>(Rules));
            }
            else
            {
                evaldDirective = new Directive(Name, Value.Evaluate(env));
            }

            evaldDirective.IsReference = IsReference;

            env.Frames.Pop();

            return evaldDirective;
        }

        public override void AppendCSS(Env env, Context context)
        {
            if (IsReference)
            {
                return;
            }

            if (env.Compress && Rules != null && !Rules.Any())
                return;

            env.Output.Append(Name);

            if (!string.IsNullOrEmpty(Identifier))
            {
                env.Output.Append(" ");
                env.Output.Append(Identifier);
            }

            if (Rules != null)
            {
                // Append pre comments as we out put each rule ourselves
                if (Rules.PreComments)
                {
                    env.Output.Append(Rules.PreComments);
                }

                AppendRules(env);
                env.Output.Append("\n");
            }
            else
            {
                env.Output
                    .Append(" ")
                    .Append(Value)
                    .Append(";\n");
            }
        }
    }
}