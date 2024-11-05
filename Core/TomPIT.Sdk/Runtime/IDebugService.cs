using System;

namespace TomPIT.Runtime;
public interface IDebugService
{
	bool Enabled { get; }

	void ConfigurationAdded(Guid component);
	void ConfigurationChanged(Guid component);
	void ConfigurationRemoved(Guid component);
	void SourceTextChanged(Guid microService, Guid configuration, Guid token, int type);
}
