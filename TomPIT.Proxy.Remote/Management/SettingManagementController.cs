using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management
{
	internal class SettingManagementController : ISettingManagementController
	{
		private const string Controller = "SettingManagement";

		public void Update(string name, string nameSpace, string type, string primaryKey, object value)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				Name = name,
				Type = type,
				PrimaryKey = primaryKey,
				Value = value,
				NameSpace = nameSpace
			});
		}
	}
}
