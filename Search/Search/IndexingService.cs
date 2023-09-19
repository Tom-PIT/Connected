using System;
using TomPIT.Search.Indexing;

namespace TomPIT.Search;

internal class IndexingService : IIndexingService
{
    public void Complete(Guid popReceipt)
    {
        Instance.SysProxy.Management.Search.Complete(popReceipt);
    }

    public void CompleteRebuilding(Guid catalog)
    {
        Instance.SysProxy.Management.Search.DeleteState(catalog);
    }

    public void Flush()
    {
        IndexCache.Flush();
    }

    public void MarkRebuilding(Guid catalog)
    {
        Instance.SysProxy.Management.Search.UpdateState(catalog, CatalogStateStatus.Rebuilding);
    }

    public void Ping(Guid popReceipt, int nextVisible)
    {
        Instance.SysProxy.Management.Search.Ping(popReceipt, nextVisible);
    }

    public void Rebuild(Guid catalog)
    {
        Instance.SysProxy.Management.Search.InvalidateState(catalog);
    }

    public void ResetRebuilding(Guid catalog)
    {
        Instance.SysProxy.Management.Search.UpdateState(catalog, CatalogStateStatus.Pending);
    }

    public void Scave()
    {
        IndexCache.Scave();
    }

    public ICatalogState SelectState(Guid catalog)
    {
        return Instance.SysProxy.Management.Search.SelectState(catalog);
    }
}
