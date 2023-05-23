using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Environment;

namespace TomPIT.Management.Environment
{
    internal class ResourceGroupManagementService : TenantObject, IResourceGroupManagementService
    {
        public ResourceGroupManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Delete(Guid token)
        {
            Instance.SysProxy.Management.ResourceGroups.Delete(token);
        }

        public Guid Insert(string name, Guid storageProvider, string connectionString)
        {
            return Instance.SysProxy.Management.ResourceGroups.Insert(name, storageProvider, connectionString);
        }

        public void Update(Guid token, string name, Guid storageProvider, string connectionString)
        {
            Instance.SysProxy.Management.ResourceGroups.Update(token, name, storageProvider, connectionString);
        }

        public List<IServerResourceGroup> Query()
        {
            return Instance.SysProxy.Management.ResourceGroups.Query().ToList();
        }
    }
}
