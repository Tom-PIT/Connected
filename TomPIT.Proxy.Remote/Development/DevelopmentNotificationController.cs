using System;
using TomPIT.Proxy.Development;

namespace TomPIT.Proxy.Remote.Development
{
	internal class DevelopmentNotificationController : IDevelopmentNotificationController
	{
		private const string Controller = "NotificationDevelopment";
		public void ConfigurationAdded(Guid microService, Guid configuration, string category)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ConfigurationAdded"), new
			{
				microService,
				configuration,
				category
			});
		}

		public void ConfigurationChanged(Guid microService, Guid configuration, string category)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ConfigurationChanged"), new
			{
				microService,
				configuration,
				category
			});
		}

		public void ConfigurationRemoved(Guid microService, Guid configuration, string category)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ConfigurationRemoved"), new
			{
				microService,
				configuration,
				category
			});
		}

		public void MicroServiceInstalled(Guid microService, bool success)
		{
			Connection.Post(Connection.CreateUrl(Controller, "MicroServiceInstalled"), new
			{
				microService,
				success
			});
		}

		public void SourceTextChanged(Guid microService, Guid configuration, Guid token, int type)
		{
			Connection.Post(Connection.CreateUrl(Controller, "SourceTextChanged"), new
			{
				microService,
				configuration,
				token,
				type
			});
		}
	}
}
