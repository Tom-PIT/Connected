using System;
using System.Collections.Immutable;
using TomPIT.Cdn;

namespace TomPIT.Proxy.Management;

public interface IEventManagementController
{
    ImmutableList<IEventQueueMessage> Dequeue(int count);
    IEventDescriptor Select(Guid id);
    void Complete(Guid popReceipt);
    void Ping(Guid popReceipt, int nextVisible);
}
