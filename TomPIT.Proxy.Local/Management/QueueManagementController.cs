using System;
using System.Collections.Immutable;
using TomPIT.Proxy.Management;
using TomPIT.Storage;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Cdn;

namespace TomPIT.Proxy.Local.Management;
internal class QueueManagementController : IQueueManagementController
{
    public void Complete(Guid popReceipt)
    {
        DataModel.Queue.Complete(popReceipt);
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(5), QueueScope.Content, QueueingModel.Queue).ToImmutableList();
    }

    public void Ping(Guid popReceipt, TimeSpan nextVisible)
    {
        DataModel.Queue.Ping(popReceipt, nextVisible);
    }
}
