using System.Text.RegularExpressions;
using TomPIT.ComponentModel.Data;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Model : ComponentCreateHandler<IModelConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Model.txt";

		protected override string OnReplace(Match match)
		{
			if (string.Compare(match.Value, "[NAME]", false) == 0)
				return ComponentName;
			else if (string.Compare(match.Value, "[SCHEMANAME]", false) == 0)
				return RawComponentName;
			else if (string.Compare(match.Value, "[ENTITY]", false) == 0)
				return $"{RawComponentName}Entity";
			else if (string.Compare(match.Value, "[CACHEKEY]", false) == 0)
				return $"{RawComponentName}Entity";
			else if (string.Compare(match.Value, "[POLICYENUM]", false) == 0)
				return $"{RawComponentName}Policy";
			else if (string.Compare(match.Value, "[CLAIMS]", false) == 0)
				return $"{RawComponentName}Claim";
			else if (string.Compare(match.Value, "[AUTHKEY]", false) == 0)
				return $"{Singular(RawComponentName)}";
			else if (string.Compare(match.Value, "[POLICYATTRIBUTE]", false) == 0)
				return $"{RawComponentName}PolicyAttribute";
			else if (string.Compare(match.Value, "[AUTHMODEL]", false) == 0)
				return $"{RawComponentName}AuthorizationModel";
			else
				return base.OnReplace(match);
		}

		private string RawComponentName
		{
			get
			{
				if (ComponentName.EndsWith("model", System.StringComparison.OrdinalIgnoreCase))
					return ComponentName[0..^5];

				return ComponentName;
			}
		}
	}
}
