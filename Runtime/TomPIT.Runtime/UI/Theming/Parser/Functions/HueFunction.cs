namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class HueFunction : HslColorFunctionBase
    {
        protected override Node EvalHsl(ThemeHslColor color)
        {
            return color.GetHueInDegrees();
        }

        protected override Node EditHsl(ThemeHslColor color, Number number)
        {
            WarnNotSupportedByLessJS("hue(color, number)");

            color.Hue += number.Value/360d;
            return color.ToRgbColor();
        }
    }

    public class SpinFunction : HueFunction { }
}