using System.Text.RegularExpressions;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class DistributedEvent : ComponentCreateHandler<IDistributedEvent>
	{
		protected override string OnReplace(Match match)
		{
			if (string.Compare(match.Value, "[NAME]", false) == 0)
				return $"{Instance.Name}";

			return base.OnReplace(match);
		}

		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.DistributedEvent.txt";
	}
}
