using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class EventManagementController : IEventManagementController
{
    public void Complete(Guid popReceipt)
    {
        DataModel.Events.Complete(popReceipt);
    }

    public ImmutableList<IEventQueueMessage> Dequeue(int count)
    {
        return DataModel.Events.Dequeue(count);
    }

    public void Ping(Guid popReceipt, int nextVisible)
    {
        DataModel.Events.Ping(popReceipt, TimeSpan.FromSeconds(nextVisible));
    }

    public IEventDescriptor Select(Guid id)
    {
        return DataModel.Events.Select(id);
    }
}
