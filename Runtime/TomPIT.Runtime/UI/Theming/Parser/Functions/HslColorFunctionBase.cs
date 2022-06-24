namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public abstract class HslColorFunctionBase : ColorFunctionBase
    {
        protected override Node Eval(Color color)
        {
            var hsl = ThemeHslColor.FromRgbColor(color);

            return EvalHsl(hsl);
        }

        protected override Node EditColor(Color color, Number number)
        {
            var hsl = ThemeHslColor.FromRgbColor(color);

            return EditHsl(hsl, number);
        }

        protected abstract Node EvalHsl(ThemeHslColor color);

        protected abstract Node EditHsl(ThemeHslColor color, Number number);
    }
}