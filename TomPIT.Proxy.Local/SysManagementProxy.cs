using TomPIT.Proxy.Local.Management;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Local
{
	internal class SysManagementProxy : ISysManagementProxy
	{
		public SysManagementProxy()
		{
			Roles = new RoleManagementController();
			Settings = new SettingManagementController();
			BigData = new BigDataManagementController();
			Users = new UserManagementController();
			MicroServices = new MicroServiceManagementController();
		}

		public IRoleManagementController Roles { get; }
		public ISettingManagementController Settings { get; }
		public IBigDataManagementController BigData { get; }
		public IUserManagementController Users { get; }
		public IMicroServiceManagementController MicroServices { get; }
	}
}
