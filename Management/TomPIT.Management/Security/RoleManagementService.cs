using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	internal class RoleManagementService : TenantObject, IRoleManagementService
	{
		public RoleManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid token)
		{
			var u = Tenant.CreateUrl("RoleManagement", "Delete");
			var e = new JObject
			{
				{"token", token}
			};

			Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}

		public Guid Insert(string name)
		{
			var u = Tenant.CreateUrl("RoleManagement", "Insert");
			var e = new JObject
			{
				{"name", name}
			};

			var id = Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(id));

			return id;
		}

		public void Update(Guid token, string name)
		{
			var u = Tenant.CreateUrl("RoleManagement", "Update");
			var e = new JObject
			{
				{"name", name},
				{"token", token}
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}
	}
}
