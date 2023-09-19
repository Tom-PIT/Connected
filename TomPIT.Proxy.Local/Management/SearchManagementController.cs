using System;
using System.Collections.Immutable;
using TomPIT.Proxy.Management;
using TomPIT.Search;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class SearchManagementController : ISearchManagementController
{
    public void Complete(Guid popReceipt)
    {
        DataModel.Search.Complete(popReceipt);
    }

    public void DeleteState(Guid catalog)
    {
        DataModel.Search.DeleteState(catalog);
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return DataModel.Search.Dequeue(count);
    }

    public void InvalidateState(Guid catalog)
    {
        DataModel.Search.InvalidateState(catalog);
    }

    public void Ping(Guid popReceipt, int nextVisible)
    {
        DataModel.Search.Ping(popReceipt, TimeSpan.FromSeconds(nextVisible));
    }

    public IIndexRequest Select(Guid id)
    {
        return DataModel.Search.Select(id);
    }

    public ICatalogState SelectState(Guid catalog)
    {
        return DataModel.Search.SelectState(catalog);
    }

    public void UpdateState(Guid catalog, CatalogStateStatus status)
    {
        DataModel.Search.UpdateState(catalog, status);
    }
}
