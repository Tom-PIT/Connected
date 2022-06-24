﻿namespace TomPIT.UI.Theming.Parser.Functions
{
	using System;
	using System.Linq;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Utils;

	public class PowFunction : Function
	{
		protected override Node Evaluate(Env env)
		{
			Guard.ExpectMinArguments(2, Arguments.Count, this, Location);
			Guard.ExpectMaxArguments(2, Arguments.Count, this, Location);
			Guard.ExpectAllNodes<Number>(Arguments, this, Location);

			WarnNotSupportedByLessJS("pow(number, number)");

			var first = Arguments.Cast<Number>().First();
			var second = Arguments.Cast<Number>().ElementAt(1);
			var value = Math.Pow(first.Value, second.Value);

			return new Number(value);
		}
	}
}
