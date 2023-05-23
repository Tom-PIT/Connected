using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Proxy.Management;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class SubscriptionManagementController : ISubscriptionManagementController
{
    public void Complete(Guid popReceipt)
    {
        DataModel.Subscriptions.Complete(popReceipt);
    }

    public void CompleteEvent(Guid popReceipt)
    {
        DataModel.Subscriptions.CompleteEvent(popReceipt);
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return DataModel.Subscriptions.Dequeue(count).ToImmutableList();
    }

    public ImmutableList<IQueueMessage> DequeueEvents(int count)
    {
        return DataModel.Subscriptions.DequeueEvents(count).ToImmutableList();
    }

    public void Ping(Guid popReceipt)
    {
        DataModel.Subscriptions.Ping(popReceipt);
    }

    public ISubscriptionEvent SelectEvent(Guid token)
    {
        return DataModel.Subscriptions.SelectEvent(token);
    }
}
