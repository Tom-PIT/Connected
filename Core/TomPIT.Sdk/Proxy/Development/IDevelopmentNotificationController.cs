using System;

namespace TomPIT.Proxy.Development
{
	public interface IDevelopmentNotificationController
	{
		void MicroServiceInstalled(Guid microService, bool success);
		void SourceTextChanged(Guid microService, Guid configuration, Guid token, int type);
		void ConfigurationRemoved(Guid microService, Guid configuration, string category);
		void ConfigurationAdded(Guid microService, Guid configuration, string category);
		void ConfigurationChanged(Guid microService, Guid configuration, string category);
	}
}
