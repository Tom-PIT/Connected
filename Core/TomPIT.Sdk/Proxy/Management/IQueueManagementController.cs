using System;
using System.Collections.Immutable;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management
{
    public interface IQueueManagementController
    {
        ImmutableList<IQueueMessage> Dequeue(int count);
        void Ping(Guid popReceipt, TimeSpan nextVisible);
        void Complete(Guid popReceipt);
    }
}
