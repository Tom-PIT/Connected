using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;

namespace TomPIT.Configuration
{
	internal class SettingManagementService : ISettingManagementService
	{
		public SettingManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

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

			var u = Connection.CreateUrl("SettingManagement", "Update");

			Connection.Post(u, d);

			if (Connection.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(resourceGroup, name));
		}

		public void Delete(Guid resourceGroup, string name)
		{
			var d = new JObject
			{
				{"resourceGroup", resourceGroup },
				{"name", name }
			};

			var u = Connection.CreateUrl("SettingManagement", "Delete");

			Connection.Post(u, d);

			if (Connection.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(resourceGroup, name));
		}
	}
}
