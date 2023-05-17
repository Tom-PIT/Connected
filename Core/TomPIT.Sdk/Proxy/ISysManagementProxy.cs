using TomPIT.Proxy.Management;

namespace TomPIT.Proxy
{
	public interface ISysManagementProxy
	{
		IRoleManagementController Roles { get; }
		ISettingManagementController Settings { get; }
		IBigDataManagementController BigData { get; }
		IUserManagementController Users { get; }
		IMicroServiceManagementController MicroServices { get; }
	}
}
