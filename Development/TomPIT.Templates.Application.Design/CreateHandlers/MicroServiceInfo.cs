using System;
using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class MicroServiceInfo : ComponentCreateHandler<IMicroServiceInfoConfiguration>
	{
		protected override string OnReplace(Match match)
		{
			if (string.Compare(match.Value, "[NAME]", false) == 0)
				return $"{Instance.ComponentName()}";
			else if (string.Compare(match.Value, "[VERSION]", false) == 0)
				return $"{DateTime.Today.Month}{DateTime.Today.Day:00}";

			return base.OnReplace(match);
		}

		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.MicroServiceInfo.txt";
	}
}