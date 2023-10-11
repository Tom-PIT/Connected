using System;

namespace TomPIT.Runtime;
public interface IDebugService
{
	bool Enabled { get; }

	void ConfigurationAdded(Guid microService, Guid component, string category);
	void ConfigurationChanged(Guid microService, Guid component, string category);
	void ConfigurationRemoved(Guid microService, Guid component, string category);
	void ScriptChanged(Guid microService, Guid component, Guid element);
}
