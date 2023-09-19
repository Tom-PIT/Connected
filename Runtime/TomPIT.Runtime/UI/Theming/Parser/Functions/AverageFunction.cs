﻿namespace TomPIT.UI.Theming.Parser.Functions
{
	public class AverageFunction : ColorMixFunction
    {
        protected override double Operate(double a, double b)
        {
            return (a + b) / 2;
        }
    }
}
