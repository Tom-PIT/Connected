using System;

namespace TomPIT.Runtime;
public interface IDebugService
{
	bool Enabled { get; }

	void ConfigurationAdded(Guid component);
	void ConfigurationChanged(Guid component);
	void ConfigurationRemoved(Guid component);
	void ScriptChanged(Guid microService, Guid component, Guid element, Guid blob);
}
