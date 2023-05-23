using System;
using System.Collections.Immutable;
using TomPIT.Environment;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class ResourceGroupManagementController : IResourceGroupManagementController
{
    public void Delete(Guid token)
    {
        DataModel.ResourceGroups.Delete(token);
    }

    public Guid Insert(string name, Guid storageProvider, string connectionString)
    {
        return DataModel.ResourceGroups.Insert(name, storageProvider, connectionString);
    }

    public ImmutableList<IServerResourceGroup> Query()
    {
        return DataModel.ResourceGroups.Query();
    }

    public void Update(Guid token, string name, Guid storageProvider, string connectionString)
    {
        DataModel.ResourceGroups.Update(token, name, storageProvider, connectionString);
    }
}
