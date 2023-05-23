using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management;
internal class QueueManagementController : IQueueManagementController
{
    private const string Controller = "QueueManagement";
    public void Complete(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Complete"), new
        {
            popReceipt
        });
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "Dequeue"), new
        {
            count
        }).ToImmutableList<IQueueMessage>();
    }

    public void Ping(Guid popReceipt, TimeSpan nextVisible)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
        {
            popReceipt,
            nextVisible
        });

    }
}
