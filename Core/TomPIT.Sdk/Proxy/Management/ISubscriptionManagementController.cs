using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management
{
    public interface ISubscriptionManagementController
    {
        ImmutableList<IQueueMessage> Dequeue(int count);
        ImmutableList<IQueueMessage> DequeueEvents(int count);
        ISubscriptionEvent SelectEvent(Guid token);
        void Complete(Guid popReceipt);
        void Ping(Guid popReceipt);
        void CompleteEvent(Guid popReceipt);
    }
}
