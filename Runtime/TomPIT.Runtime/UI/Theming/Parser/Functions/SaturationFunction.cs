namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class SaturateFunction : HslColorFunctionBase
    {
        protected override Node EvalHsl(ThemeHslColor color)
        {
            return color.GetSaturation();
        }

        protected override Node EditHsl(ThemeHslColor color, Number number)
        {
            color.Saturation += number.Value/100;
            return color.ToRgbColor();
        }
    }

    public class DesaturateFunction : SaturateFunction
    {
        protected override Node EditHsl(ThemeHslColor color, Number number)
        {
            return base.EditHsl(color, -number);
        }
    }

    public class SaturationFunction : SaturateFunction 
    {
        protected override Node EditHsl(ThemeHslColor color, Number number)
        {
            WarnNotSupportedByLessJS("saturation(color, number)", "saturate(color, number) or its opposite desaturate(color, number),");

            return base.EditHsl(color, number);
        }
    }

}