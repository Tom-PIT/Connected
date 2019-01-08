using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Environment
{
	internal class ResourceGroupManagementService : IResourceGroupManagementService
	{
		public ResourceGroupManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid token)
		{
			var u = Connection.CreateUrl("ResourceGroupManagement", "Delete");
			var d = new JObject
			{
				{"token",token }
			};

			Connection.Post(u, d);
		}

		public Guid Insert(string name, Guid storageProvider, string connectionString)
		{
			var u = Connection.CreateUrl("ResourceGroupManagement", "Insert");
			var d = new JObject
			{
				{"name",name },
				{"storageProvider",storageProvider },
				{"connectionString",connectionString }
			};

			return Connection.Post<Guid>(u, d);
		}

		public void Update(Guid token, string name, Guid storageProvider, string connectionString)
		{
			var u = Connection.CreateUrl("ResourceGroupManagement", "Update");
			var d = new JObject
			{
				{"token",token },
				{ "name",name },
				{"storageProvider",storageProvider },
				{"connectionString",connectionString }
			};

			Connection.Post(u, d);
		}

		public List<ManagementResourceGroup> Query()
		{
			var u = Connection.CreateUrl("ResourceGroupManagement", "Query");

			return Connection.Get<List<ManagementResourceGroup>>(u);
		}
	}
}
