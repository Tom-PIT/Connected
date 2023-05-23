using System.Collections.Immutable;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management
{
    public interface IStorageManagementController
    {
        ImmutableList<IBlob> QueryOrphanedDrafts();
        ImmutableList<IClientStorageProvider> QueryStorageProviders();
    }
}
