using System.Text.RegularExpressions;
using TomPIT.ComponentModel.UI;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Partial : ComponentCreateHandler<IPartialViewConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Partial.txt";

		protected override string OnReplace(Match match)
		{
			if (string.Compare(match.Value, "[NAME]", false) == 0)
				return ComponentName;
			else if (string.Compare(match.Value, "[MICROSERVICE]", false) == 0)
				return MicroService.Name;
			else
				return base.OnReplace(match);
		}
	}
}
