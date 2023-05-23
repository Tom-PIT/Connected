using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management;
internal class StorageManagementController : IStorageManagementController
{
    private const string Controller = "StorageManagement";
    public ImmutableList<IBlob> QueryOrphanedDrafts()
    {
        return Connection.Get<List<Blob>>(Connection.CreateUrl(Controller, "QueryOrphanedDrafts")).ToImmutableList<IBlob>();
    }

    public ImmutableList<IClientStorageProvider> QueryStorageProviders()
    {
        return Connection.Get<List<ClientStorageProvider>>(Connection.CreateUrl(Controller, "QueryStorageProviders")).ToImmutableList<IClientStorageProvider>();
    }
}
