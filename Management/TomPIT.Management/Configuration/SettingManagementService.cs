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

		public void Delete(string name, string nameSpace, string type, string primaryKey)
		{
			var d = new JObject
			{
				{"name", name },
				{"type", type },
				{"nameSpace", nameSpace },
				{"primaryKey", primaryKey }
			};

			var u = Tenant.CreateUrl("SettingManagement", "Delete");

			Tenant.Post(u, d);

			if (Tenant.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(name, nameSpace, type, primaryKey));
		}
	}
}
