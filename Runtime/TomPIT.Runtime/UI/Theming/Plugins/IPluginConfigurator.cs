namespace TomPIT.UI.Theming.Plugins
{
	using System;
	using System.Collections.Generic;

	public interface IPluginConfigurator
    {
        IPlugin CreatePlugin();

        IEnumerable<IPluginParameter> GetParameters();

        void SetParameterValues(IEnumerable<IPluginParameter> parameters);

        string Name { get; }

        string Description { get; }

        Type Configurates { get; }
    }
}
