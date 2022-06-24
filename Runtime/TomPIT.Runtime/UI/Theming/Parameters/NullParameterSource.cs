using System.Collections.Generic;

namespace TomPIT.UI.Theming.Parameters
{
	public class NullParameterSource : IParameterSource
    {
        public IDictionary<string, string> GetParameters()
        {
            return new Dictionary<string, string>();
        }
    }
}