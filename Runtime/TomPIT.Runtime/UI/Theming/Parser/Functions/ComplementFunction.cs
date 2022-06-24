namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class ComplementFunction : HslColorFunctionBase
    {
        protected override Node EvalHsl(ThemeHslColor color)
        {
            WarnNotSupportedByLessJS("complement(color)");

            color.Hue += 0.5;
            return color.ToRgbColor();
        }

        protected override Node EditHsl(ThemeHslColor color, Number number)
        {
            return null;
        }
    }
}