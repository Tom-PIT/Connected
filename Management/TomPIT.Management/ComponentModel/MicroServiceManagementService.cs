using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceManagementService : IMicroServiceManagementService
	{
		public MicroServiceManagementService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Delete(Guid microService)
		{
			var u = Server.CreateUrl("MicroServiceManagement", "Delete");
			var args = new JObject {
				{"microService", microService }
			};

			Server.Connection.Post(u, args);

			if (Server.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyRemoved(this, new MicroServiceEventArgs(microService));
		}

		public Guid Insert(string name, Guid resourceGroup, Guid template, MicroServiceStatus status)
		{
			var token = Guid.NewGuid();

			var u = Server.CreateUrl("MicroServiceManagement", "Insert");
			var args = new JObject
			{
				{ "name",name },
				{ "microService",token },
				{"status", status.ToString() },
				{"resourceGroup", resourceGroup },
				{"template", template },
				{"meta", CreateMicroServiceMeta(token) }
			};

			Server.Connection.Post(u, args);

			if (Shell.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(token));

			return token;
		}

		private string CreateMicroServiceMeta(Guid microService)
		{
			var u = Server.CreateUrl("MicroServiceManagement", "CreateMicroServiceMeta")
				.AddParameter("microService", microService);

			return Server.Connection.Get<string>(u);
		}

		public void Update(Guid microService, string name, MicroServiceStatus status, Guid resourceGroup)
		{
			var u = Server.CreateUrl("MicroServiceManagement", "Update");
			var args = new JObject
			{
				{ "name",name },
				{ "microService",microService },
				{"status", status.ToString() },
				{"resourceGroup", resourceGroup },
			};

			Server.Connection.Post(u, args);

			if (Shell.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(microService));
		}

		public ListItems<IMicroService> Query(Guid resourceGroup)
		{
			var u = Server.CreateUrl("MicroServiceManagement", "Query")
				.AddParameter("resourceGroup", resourceGroup);

			return Server.Connection.Get<List<MicroService>>(u).ToList<IMicroService>();
		}
	}
}
