using TomPIT.Configuration;
using TomPIT.Connectivity;

namespace TomPIT.Management.Configuration
{
    internal class SettingManagementService : TenantObject, ISettingManagementService
    {
        public SettingManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Delete(string name, string nameSpace, string type, string primaryKey)
        {
            Instance.SysProxy.Management.Settings.Delete(name, nameSpace, type, primaryKey);

            if (Tenant.GetService<ISettingService>() is ISettingNotification n)
                n.NotifyChanged(this, new SettingEventArgs(name, nameSpace, type, primaryKey));
        }
    }
}
