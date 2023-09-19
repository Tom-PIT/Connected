using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management;
internal class SubscriptionManagementController : ISubscriptionManagementController
{
    private const string Controller = "SubscriptionManagement";

    public void Complete(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Complete"), new
        {
            popReceipt
        });
    }

    public void CompleteEvent(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "CompleteEvent"), new
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

    public ImmutableList<IQueueMessage> DequeueEvents(int count)
    {
        return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "DequeueEvents"), new
        {
            count
        }).ToImmutableList<IQueueMessage>();
    }

    public void Ping(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
        {
            popReceipt
        });
    }

    public ISubscriptionEvent SelectEvent(Guid token)
    {
        return Connection.Post<SubscriptionEventDescriptor>(Connection.CreateUrl(Controller, "SelectEvent"), new
        {
            token
        });
    }
}
