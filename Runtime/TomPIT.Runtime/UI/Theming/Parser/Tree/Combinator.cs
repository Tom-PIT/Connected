﻿namespace TomPIT.UI.Theming.Parser.Tree
{
	using Infrastructure;
	using Infrastructure.Nodes;

	public class Combinator : Node
    {
        public string Value { get; set; }

        public Combinator(string value)
        {
            if (string.IsNullOrEmpty(value))
                Value = "";
            else if (value == " ")
                Value = " ";
            else
                Value = value.Trim();
        }

        protected override Node CloneCore() {
            return new Combinator(Value);
        }

        public override void AppendCSS(Env env)
        {
            env.Output.Append(GetValue(env));
        }

        private string GetValue(Env env) {
            switch (Value) {
                case "+":
                    return env.Compress ? "+" : " + ";
                case "~":
                    return env.Compress ? "~" : " ~ ";
                case ">":
                    return env.Compress ? ">" : " > ";
                default:
                    return Value;
            }
        }
    }
}