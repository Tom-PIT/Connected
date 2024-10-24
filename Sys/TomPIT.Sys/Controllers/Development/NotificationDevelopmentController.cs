﻿using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Model;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Controllers.Development
{
	public class NotificationDevelopmentController : SysController
	{
		[HttpPost]
		public void ConfigurationAdded()
		{
			var body = FromBody();

			var configuration = body.Required<Guid>("configuration");

			var c = DataModel.Components.Select(configuration);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			CachingNotifications.ConfigurationAdded(c.MicroService, c.Token, c.Category);
		}

		[HttpPost]
		public void ConfigurationChanged()
		{
			var body = FromBody();

			var configuration = body.Required<Guid>("configuration");

			var c = DataModel.Components.Select(configuration);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			DataModel.Components.UpdateModified(c.MicroService, c.Category, c.Name);
			CachingNotifications.ConfigurationChanged(c.MicroService, c.Token, c.Category);
		}

		[HttpPost]
		public void ConfigurationRemoved()
		{
			var body = FromBody();

			var configuration = body.Required<Guid>("configuration");
			var microService = body.Required<Guid>("microService");
			var category = body.Required<string>("category");

			CachingNotifications.ConfigurationRemoved(microService, configuration, category);
		}

		[HttpPost]
		public void ScriptChanged()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var container = body.Required<Guid>("container");
			var sourceCode = body.Required<Guid>("sourceCode");

			CachingNotifications.ScriptChanged(microService, container, sourceCode);
		}

		[HttpPost]
		public void MicroServiceInstalled()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var success = body.Required<bool>("success");

			CachingNotifications.MicroServiceInstalled(microService, success);
		}
	}
}
