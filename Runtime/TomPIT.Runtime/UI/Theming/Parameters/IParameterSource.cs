namespace TomPIT.UI.Theming.Parameters
{
	using System.Collections.Generic;

	public interface IParameterSource
    {
        IDictionary<string, string> GetParameters();
    }
}