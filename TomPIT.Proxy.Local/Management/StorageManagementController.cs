using System;
using System.Collections.Immutable;
using TomPIT.Api.Storage;
using TomPIT.Proxy.Management;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Proxy.Local.Management;
internal class StorageManagementController : IStorageManagementController
{
    public ImmutableList<IBlob> QueryOrphanedDrafts()
    {
        return Shell.GetService<IDatabaseService>().Proxy.Storage.QueryOrphaned(DateTime.UtcNow.AddDays(-1)).ToImmutableList();
    }

    public ImmutableList<IClientStorageProvider> QueryStorageProviders()
    {
        return Shell.GetService<IStorageProviderService>().Query().ToImmutableList<IClientStorageProvider>();
    }
}
