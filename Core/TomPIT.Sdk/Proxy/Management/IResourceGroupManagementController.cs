using System;
using System.Collections.Immutable;
using TomPIT.Environment;

namespace TomPIT.Proxy.Management;

public interface IResourceGroupManagementController
{
    Guid Insert(string name, Guid storageProvider, string connectionString);
    void Update(Guid token, string name, Guid storageProvider, string connectionString);
    void Delete(Guid token);
    ImmutableList<IServerResourceGroup> Query();
}
