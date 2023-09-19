using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Environment;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class ResourceGroupManagementController : IResourceGroupManagementController
{
    private const string Controller = "ResourceGroupManagement";
    public void Delete(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
        {
            token
        });
    }

    public Guid Insert(string name, Guid storageProvider, string connectionString)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Insert"), new
        {
            name,
            storageProvider,
            connectionString
        });
    }

    public ImmutableList<IServerResourceGroup> Query()
    {
        return Connection.Get<List<ManagementResourceGroup>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IServerResourceGroup>();
    }

    public void Update(Guid token, string name, Guid storageProvider, string connectionString)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Update"), new
        {
            token,
            name,
            storageProvider,
            connectionString
        });
    }
}
