using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Search;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management;
internal class SearchManagementController : ISearchManagementController
{
    private const string Controller = "SearchManagement";

    public void Complete(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Complete"), new
        {
            popReceipt
        });
    }

    public void DeleteState(Guid catalog)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteState"), new
        {
            catalog
        });
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "Dequeue"), new
        {
            count
        }).ToImmutableList<IQueueMessage>();
    }

    public void InvalidateState(Guid catalog)
    {
        Connection.Post(Connection.CreateUrl(Controller, "InvalidateState"), new
        {
            catalog
        });
    }

    public void Ping(Guid popReceipt, int nextVisible)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
        {
            popReceipt,
            nextVisible
        });
    }

    public IIndexRequest Select(Guid id)
    {
        return Connection.Get<IndexRequest>(Connection.CreateUrl(Controller, "Select").AddParameter("id", id));
    }

    public ICatalogState SelectState(Guid catalog)
    {
        return Connection.Post<CatalogState>(Connection.CreateUrl(Controller, "SelectState"), new
        {
            catalog
        });
    }

    public void UpdateState(Guid catalog, CatalogStateStatus status)
    {
        Connection.Post(Connection.CreateUrl(Controller, "UpdateState"), new
        {
            catalog,
            status
        });
    }
}
