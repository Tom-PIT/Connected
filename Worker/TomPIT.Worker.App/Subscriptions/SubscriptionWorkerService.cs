using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
    internal class SubscriptionWorkerService : TenantObject, ISubscriptionWorkerService
    {
        public SubscriptionWorkerService(ITenant tenant) : base(tenant)
        {
        }

        public void CompleteSubscription(Guid popReceipt)
        {
            Instance.SysProxy.Management.Subscriptions.Complete(popReceipt);
        }

        public void CompleteEvent(Guid popReceipt)
        {
            Instance.SysProxy.Management.Subscriptions.CompleteEvent(popReceipt);
        }

        public void InsertSubscribers(Guid token, List<IRecipient> recipients)
        {
            if (recipients == null || recipients.Count == 0)
                return;

            var items = new List<IRecipient>();

            foreach (var recipient in recipients)
            {
                items.Add(new Recipient
                {
                    Type = recipient.Type,
                    ResourcePrimaryKey = recipient.ResourcePrimaryKey,
                    Tags = recipient.Tags
                });
            }

            Instance.SysProxy.Subscriptions.InsertSubscribers(token, items);
        }

        public void PingSubscription(Guid popReceipt)
        {
            Instance.SysProxy.Management.Subscriptions.Ping(popReceipt);
        }

        public void PingSubscriptionEvent(Guid popReceipt)
        {
            Instance.SysProxy.Management.Subscriptions.Ping(popReceipt);
        }

        public ISubscriptionEvent SelectEvent(Guid token)
        {
            return Instance.SysProxy.Management.Subscriptions.SelectEvent(token);
        }

        public List<IRecipient> QueryRecipients(Guid subscription, string primaryKey, string topic)
        {
            return Instance.SysProxy.Subscriptions.QuerySubscribers(subscription, primaryKey, topic).ToList();
        }

        public List<IQueueMessage> DequeueSubscriptions(string resourceGroup, int count)
        {
            return Instance.SysProxy.Management.Subscriptions.Dequeue(count).ToList();
        }

        public List<IQueueMessage> DequeueSubscriptionEvents(string resourceGroup, int count)
        {
            return Instance.SysProxy.Management.Subscriptions.DequeueEvents(count).ToList();
        }
    }
}
