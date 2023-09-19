using System;

namespace TomPIT.Proxy.Development
{
	public interface IDevelopmentNotificationController
	{
		void MicroServiceInstalled(Guid microService, bool success);
		void ScriptChanged(Guid microService, Guid component, Guid element);
		void ConfigurationRemoved(Guid microService, Guid configuration, string category);
		void ConfigurationAdded(Guid microService, Guid configuration, string category);
		void ConfigurationChanged(Guid microService, Guid configuration, string category);
	}
}
