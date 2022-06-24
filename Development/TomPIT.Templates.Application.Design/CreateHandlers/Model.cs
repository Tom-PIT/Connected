using System.Linq;
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
            else if (string.Compare(match.Value, "[SCHEMA]", false) == 0)
                return ShortMicroserviceName;
            else if (string.Compare(match.Value, "[SCHEMANAME]", false) == 0)
                return SchemaName;
            else if (string.Compare(match.Value, "[MODELTYPE]", false) == 0)
                return $"{RawComponentName}sModel";
            else if (string.Compare(match.Value, "[ENTITYTYPE]", false) == 0)
                return $"{RawComponentName}Entity";
            else if (string.Compare(match.Value, "[ENTITYNAME]", false) == 0)
                return $"{RawComponentName}";
            else if (string.Compare(match.Value, "[ENTITYKEY]", false) == 0)
                return $"{RawComponentName}";
            else if (string.Compare(match.Value, "[AUTHKEY]", false) == 0)
                return $"{RawComponentName}";
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
                {
                    return Singular(ComponentName[0..^5]);
                }

                return ComponentName;
            }
        }

        private string SchemaName 
        {
            get 
            {
                if (string.IsNullOrWhiteSpace(RawComponentName))
                    return RawComponentName;

                return char.ToLower(RawComponentName[0]) + RawComponentName[1..];
            }
        }

        private string ShortMicroserviceName
        {
            get
            {
                var consonants = new[] { 'a', 'e', 'i', 'o', 'u' };
                var microserviceName = MicroService.Name.ToLower().Split(".").LastOrDefault() ?? string.Empty;
                return new string(microserviceName.Where(e => !consonants.Contains(e)).Take(3).ToArray());
            }
        }
    }
}
