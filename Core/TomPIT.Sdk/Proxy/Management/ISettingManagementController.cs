namespace TomPIT.Proxy.Management
{
	public interface ISettingManagementController
	{
		void Update(string name, string nameSpace, string type, string primaryKey, object value);
	}
}
