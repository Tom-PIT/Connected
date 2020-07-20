namespace TomPIT.Management.Configuration
{
	public interface ISettingManagementService
	{
		void Update(string name, string type, string primaryKey, string value);
		void Delete(string name, string type, string primaryKey);
	}
}
