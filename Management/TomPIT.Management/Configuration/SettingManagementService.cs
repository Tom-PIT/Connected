using Newtonsoft.Json.Linq;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Management.Configuration
{
	internal class SettingManagementService : TenantObject, ISettingManagementService
	{
		public SettingManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Update(string name, string type, string primaryKey, string value)
		{
			var d = new JObject
			{
				{"name",name },
				{"type",type},
				{"primaryKey",primaryKey },
				{"value", value }
			};

			var u = Tenant.CreateUrl("SettingManagement", "Update");

			Tenant.Post(u, d);

			if (Tenant.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(name, type, primaryKey));
		}

		public void Delete(string name, string type, string primaryKey)
		{
			var d = new JObject
			{
				{"name", name },
				{"type", type },
				{"primaryKey", primaryKey }
			};

			var u = Tenant.CreateUrl("SettingManagement", "Delete");

			Tenant.Post(u, d);

			if (Tenant.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(name, type, primaryKey));
		}
	}
}
