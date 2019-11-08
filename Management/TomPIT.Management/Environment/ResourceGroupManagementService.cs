using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Management.Environment
{
	internal class ResourceGroupManagementService : TenantObject, IResourceGroupManagementService
	{
		public ResourceGroupManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid token)
		{
			var u = Tenant.CreateUrl("ResourceGroupManagement", "Delete");
			var d = new JObject
			{
				{"token",token }
			};

			Tenant.Post(u, d);
		}

		public Guid Insert(string name, Guid storageProvider, string connectionString)
		{
			var u = Tenant.CreateUrl("ResourceGroupManagement", "Insert");
			var d = new JObject
			{
				{"name",name },
				{"storageProvider",storageProvider },
				{"connectionString",connectionString }
			};

			return Tenant.Post<Guid>(u, d);
		}

		public void Update(Guid token, string name, Guid storageProvider, string connectionString)
		{
			var u = Tenant.CreateUrl("ResourceGroupManagement", "Update");
			var d = new JObject
			{
				{"token",token },
				{ "name",name },
				{"storageProvider",storageProvider },
				{"connectionString",connectionString }
			};

			Tenant.Post(u, d);
		}

		public List<ManagementResourceGroup> Query()
		{
			var u = Tenant.CreateUrl("ResourceGroupManagement", "Query");

			return Tenant.Get<List<ManagementResourceGroup>>(u);
		}
	}
}
