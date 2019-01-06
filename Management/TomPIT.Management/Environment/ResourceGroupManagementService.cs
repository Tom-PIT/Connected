using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Environment
{
	internal class ResourceGroupManagementService : IResourceGroupManagementService
	{
		public ResourceGroupManagementService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Delete(Guid token)
		{
			var u = Server.CreateUrl("ResourceGroupManagement", "Delete");
			var d = new JObject
			{
				{"token",token }
			};

			Server.Connection.Post(u, d);
		}

		public Guid Insert(string name, Guid storageProvider, string connectionString)
		{
			var u = Server.CreateUrl("ResourceGroupManagement", "Insert");
			var d = new JObject
			{
				{"name",name },
				{"storageProvider",storageProvider },
				{"connectionString",connectionString }
			};

			return Server.Connection.Post<Guid>(u, d);
		}

		public void Update(Guid token, string name, Guid storageProvider, string connectionString)
		{
			var u = Server.CreateUrl("ResourceGroupManagement", "Update");
			var d = new JObject
			{
				{"token",token },
				{ "name",name },
				{"storageProvider",storageProvider },
				{"connectionString",connectionString }
			};

			Server.Connection.Post(u, d);
		}

		public List<ManagementResourceGroup> Query()
		{
			var u = Server.CreateUrl("ResourceGroupManagement", "Query");

			return Server.Connection.Get<List<ManagementResourceGroup>>(u);
		}
	}
}
