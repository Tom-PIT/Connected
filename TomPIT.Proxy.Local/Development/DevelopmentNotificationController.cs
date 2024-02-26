using System;
using TomPIT.Proxy.Development;
using TomPIT.Sys.Model;
using TomPIT.Sys.Notifications;

namespace TomPIT.Proxy.Local.Development
{
	internal class DevelopmentNotificationController : IDevelopmentNotificationController
	{
		public void ConfigurationAdded(Guid microService, Guid configuration, string category)
		{
			CachingNotifications.ConfigurationAdded(microService, configuration, category);
		}

		public void ConfigurationChanged(Guid microService, Guid configuration, string category)
		{
			var component = DataModel.Components.Select(configuration);

			if (component is not null)
				DataModel.Components.UpdateModified(microService, category, component.Name);

			CachingNotifications.ConfigurationChanged(microService, configuration, category);
		}

		public void ConfigurationRemoved(Guid microService, Guid configuration, string category)
		{
			CachingNotifications.ConfigurationRemoved(microService, configuration, category);
		}

		public void MicroServiceInstalled(Guid microService, bool success)
		{
			CachingNotifications.MicroServiceInstalled(microService, success);
		}

		public void SourceTextChanged(Guid microService, Guid component, Guid token, int type)
		{
			CachingNotifications.SourceTextChanged(microService, component, token, type);
		}
	}
}
