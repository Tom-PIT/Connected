﻿using System;
using TomPIT.Proxy.Development;
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

		public void ScriptChanged(Guid microService, Guid component, Guid element)
		{
			CachingNotifications.ScriptChanged(microService, component, element);
		}
	}
}
