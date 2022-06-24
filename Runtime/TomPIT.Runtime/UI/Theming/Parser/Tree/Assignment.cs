﻿namespace TomPIT.UI.Theming.Parser.Tree
{
	using Infrastructure;
	using Infrastructure.Nodes;

	public class Assignment : Node
    {
        public string Key { get; set; }
        public Node Value { get; set; }

        public Assignment(string key, Node value)
        {
            Key = key;
            Value = value;
        }

        public override Node Evaluate(Env env)
        {
            return new Assignment(Key, Value.Evaluate(env)) {Location = Location};
        }

        protected override Node CloneCore() {
            return new Assignment(Key, Value.Clone());
        }

        public override void AppendCSS(Env env)
        {
            env.Output
                .Append(Key)
                .Append("=")
                .Append(Value);
        }

        public override void Accept(Plugins.IVisitor visitor)
        {
            Value = VisitAndReplace(Value, visitor);
        }
    }
}