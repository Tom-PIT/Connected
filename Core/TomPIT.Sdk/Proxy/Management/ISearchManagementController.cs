using System;
using System.Collections.Immutable;
using TomPIT.Search;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management
{
    public interface ISearchManagementController
    {
        ImmutableList<IQueueMessage> Dequeue(int count);
        IIndexRequest Select(Guid id);
        void Complete(Guid popReceipt);
        void Ping(Guid popReceipt, int nextVisible);
        ICatalogState SelectState(Guid catalog);
        void UpdateState(Guid catalog, CatalogStateStatus status);
        void DeleteState(Guid catalog);
        void InvalidateState(Guid catalog);
    }
}
