using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class EventManagementController : IEventManagementController
{
    private const string Controller = "EventManagement";
    public void Complete(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Complete"), new
        {
            popReceipt
        });
    }

    public ImmutableList<IEventQueueMessage> Dequeue(int count)
    {
        return Connection.Post<List<EventQueueMessage>>(Connection.CreateUrl(Controller, "Dequeue"), new
        {
            count
        }).ToImmutableList<IEventQueueMessage>();
    }

    public void Ping(Guid popReceipt, int nextVisible)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
        {
            popReceipt,
            nextVisible
        });
    }

    public IEventDescriptor Select(Guid id)
    {
        return Connection.Get<EventDescriptor>(Connection.CreateUrl(Controller, "Select").AddParameter("id", id));
    }
}
