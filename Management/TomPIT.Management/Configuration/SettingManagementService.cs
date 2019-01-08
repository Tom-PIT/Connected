using System;
using TomPIT.Services;

namespace TomPIT.Configuration
{
	internal class SettingManagementService : ISettingManagementService
	{
		public void Update(IExecutionContext sender, Guid resourceGroup, string name, string value, bool visible, DataType dataType, string tags)
		{
			var server = sender.Connection();

			var d = new Setting
			{
				ResourceGroup = resourceGroup,
				Name = name,
				Value = value,
				Visible = visible,
				DataType = dataType,
				Tags = tags
			};

			var u = server.CreateUrl("SettingManagement", "Update");

			server.Post(u, d);
			server.Cache.Remove("setting", server.Cache.GenerateKey(resourceGroup, name));

			if (server.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(resourceGroup, name));
		}

		public void Delete(IExecutionContext context, Guid resourceGroup, string name)
		{
			var server = context.Connection();

			var d = new Setting
			{
				ResourceGroup = resourceGroup,
				Name = name
			};

			var u = server.CreateUrl("Setting", "Delete");

			server.Post(u, d);
			server.Cache.Remove("setting", server.Cache.GenerateKey(resourceGroup, name));

			if (server.GetService<ISettingService>() is ISettingNotification n)
				n.NotifyChanged(this, new SettingEventArgs(resourceGroup, name));
		}
	}
}
