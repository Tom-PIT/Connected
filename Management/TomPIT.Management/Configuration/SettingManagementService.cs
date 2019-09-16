using System;
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

		public void Update(Guid resourceGroup, string name, string value, bool visible, DataType dataType, string tags)
		{
			var d = new JObject
			{
				{"resourceGroup" , resourceGroup},
				{"name",name },
				{"value", value },
				{"visible" , visible },
				{"dataType" , dataType.ToString() },
				{"tags", tags }
			};

			var u = Tenant.CreateUrl("SettingManagement", "Update");

			Tenant.Post(u, d);

			if (Tenant.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(resourceGroup, name));
		}

		public void Delete(Guid resourceGroup, string name)
		{
			var d = new JObject
			{
				{"resourceGroup", resourceGroup },
				{"name", name }
			};

			var u = Tenant.CreateUrl("SettingManagement", "Delete");

			Tenant.Post(u, d);

			if (Tenant.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(resourceGroup, name));
		}
	}
}
