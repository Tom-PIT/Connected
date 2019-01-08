using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceManagementService : IMicroServiceManagementService
	{
		public MicroServiceManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid microService)
		{
			var u = Connection.CreateUrl("MicroServiceManagement", "Delete");
			var args = new JObject {
				{"microService", microService }
			};

			Connection.Post(u, args);

			if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyRemoved(this, new MicroServiceEventArgs(microService));
		}

		public Guid Insert(string name, Guid resourceGroup, Guid template, MicroServiceStatus status)
		{
			var token = Guid.NewGuid();

			var u = Connection.CreateUrl("MicroServiceManagement", "Insert");
			var args = new JObject
			{
				{ "name",name },
				{ "microService",token },
				{"status", status.ToString() },
				{"resourceGroup", resourceGroup },
				{"template", template },
				{"meta", CreateMicroServiceMeta(token) }
			};

			Connection.Post(u, args);

			if (Shell.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(token));

			return token;
		}

		private string CreateMicroServiceMeta(Guid microService)
		{
			var u = Connection.CreateUrl("MicroServiceManagement", "CreateMicroServiceMeta")
				.AddParameter("microService", microService);

			return Connection.Get<string>(u);
		}

		public void Update(Guid microService, string name, MicroServiceStatus status, Guid resourceGroup)
		{
			var u = Connection.CreateUrl("MicroServiceManagement", "Update");
			var args = new JObject
			{
				{ "name",name },
				{ "microService",microService },
				{"status", status.ToString() },
				{"resourceGroup", resourceGroup },
			};

			Connection.Post(u, args);

			if (Shell.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(microService));
		}

		public ListItems<IMicroService> Query(Guid resourceGroup)
		{
			var u = Connection.CreateUrl("MicroServiceManagement", "Query")
				.AddParameter("resourceGroup", resourceGroup);

			return Connection.Get<List<MicroService>>(u).ToList<IMicroService>();
		}
	}
}
