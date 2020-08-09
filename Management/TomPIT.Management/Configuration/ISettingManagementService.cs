namespace TomPIT.Management.Configuration
{
	public interface ISettingManagementService
	{
		void Delete(string name, string nameSpace, string type, string primaryKey);
	}
}
