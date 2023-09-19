﻿namespace TomPIT.UI.Theming.Parser.Tree
{
	using Infrastructure;
	using Infrastructure.Nodes;

	public class Comment : Node
    {
        public string Value { get; set; }
        public bool IsValidCss { get; set; }
        public bool IsSpecialCss { get; set; }
        public bool IsPreSelectorComment { get; set; }
        private bool IsCSSHack { get; set; }

        public Comment(string value)
        {
            Value = value;
            IsValidCss = !value.StartsWith("//");
            IsSpecialCss = value.StartsWith("/**") || value.StartsWith("/*!");
            IsCSSHack = value == "/**/" || value == "/*\\*/";
        }

        protected override Node CloneCore() {
            return new Comment(Value);
        }

        public override void AppendCSS(Env env)
        {
            if (IsReference || env.IsCommentSilent(IsValidCss, IsCSSHack, IsSpecialCss)) {
                return;
            }

            env.Output.Append(Value);

            if (!IsCSSHack && IsPreSelectorComment)
            {
                env.Output.Append("\n");
            }
        }
    }
}