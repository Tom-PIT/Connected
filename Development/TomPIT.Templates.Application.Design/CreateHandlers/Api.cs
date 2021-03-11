using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Api : ComponentCreateHandler<IApiConfiguration>
	{
		protected override string OnReplace(Match match)
		{
			if (string.Compare(match.Value, "[NAME]", false) == 0)
				return $"{Instance.ComponentName()}";

			return base.OnReplace(match);
		}

		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Api.txt";
	}
}