﻿namespace TomPIT.UI.Theming.Parser.Functions
{
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;

	public class RedFunction : ColorFunctionBase
    {
        protected override Node Eval(Color color)
        {
            return new Number(color.RGB[0]);
        }

        protected override Node EditColor(Color color, Number number)
        {
            WarnNotSupportedByLessJS("red(color, number)");

            var value = number.Value;

            if (number.Unit == "%")
                value = (value*255)/100d;

            return new Color(color.R + value, color.G, color.B);
        }
    }
}