namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;

	public class BlueFunction : ColorFunctionBase
    {
        protected override Node Eval(Color color)
        {
            return new Number(color.B);
        }

        protected override Node EditColor(Color color, Number number)
        {
            WarnNotSupportedByLessJS("blue(color, number)");

            var value = number.Value;

            if (number.Unit == "%")
                value = (value*255)/100d;

            return new Color(color.R, color.G, color.B + value);
        }
    }
}