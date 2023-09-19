namespace TomPIT.UI.Theming.Parser.Functions
{
	using System;
	using Exceptions;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class ColorFunction : Function
    {
        protected override Node Evaluate(Env env)
        {
            Guard.ExpectNumArguments(1, Arguments.Count, this, Location);
            var node = Guard.ExpectNode<TextNode>(Arguments[0], this, Location);

            try
            {
                return Color.From(node.Value);
            }
            catch (FormatException ex)
            {
                var message = string.Format("Invalid RGB color string '{0}'", node.Value);
                throw new ParsingException(message, ex, Location, null);
            }
        }
    }
}