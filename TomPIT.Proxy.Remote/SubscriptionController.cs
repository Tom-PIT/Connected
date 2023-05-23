using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;

namespace TomPIT.Proxy.Remote
{
    internal class SubscriptionController : ISubscriptionController
    {
        private const string Controller = "Subscription";

        public void CreateSubscription(Guid microService, Guid configuration, string primaryKey, string topic)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Enqueue"), new
            {
                microService,
                handler = configuration,
                primaryKey,
                topic
            });
        }

        public void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
        {
            Connection.Post(Connection.CreateUrl(Controller, "DeleteSubscriber"), new
            {
                subscription,
                type = type.ToString(),
                resourcePrimaryKey
            });
        }

        public void DeleteSubscription(Guid configuration, string primaryKey, string topic)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
            {
                handler = configuration,
                primaryKey,
                topic
            });
        }

        public Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags = null)
        {
            return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertSubscriber"), new
            {
                subscription,
                type,
                resourcePrimaryKey,
                tags
            });
        }

        public void InsertSubscribers(Guid subscription, List<IRecipient> recipients)
        {
            Connection.Post(Connection.CreateUrl(Controller, "InsertSubscribers"), new
            {
                subscription,
                recipients
            });
        }

        public ImmutableList<IRecipient> QuerySubscribers(Guid configuration, string primaryKey, string topic)
        {
            return Connection.Post<List<Recipient>>(Connection.CreateUrl(Controller, "QuerySubscribers"), new
            {
                handler = configuration,
                primaryKey,
                topic
            }).ToImmutableList<IRecipient>();
        }

        public IRecipient SelectSubscriber(Guid token)
        {
            return Connection.Post<Recipient>(Connection.CreateUrl(Controller, "SelectSubscriberByToken"), new
            {
                token
            });
        }

        public IRecipient SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
        {
            return Connection.Post<Recipient>(Connection.CreateUrl(Controller, "SelectSubscriberBySubscription"), new
            {
                subscription,
                type = type.ToString(),
                resourcePrimaryKey
            });
        }

        public ISubscription SelectSubscription(Guid token)
        {
            return Connection.Post<SubscriptionDescriptor>(Connection.CreateUrl(Controller, "SelectSubscription"), new
            {
                token
            });
        }

        public ISubscription SelectSubscription(Guid configuration, string primaryKey)
        {
            return Connection.Post<SubscriptionDescriptor>(Connection.CreateUrl(Controller, "SelectSubscriptionByConfiguration"), new
            {
                handler = configuration,
                primaryKey
            });
        }

        public bool SubscriptionExists(Guid configuration, string primaryKey, string topic)
        {
            return Connection.Post<bool>(Connection.CreateUrl(Controller, "Exists"), new
            {
                handler = configuration,
                primaryKey,
                topic
            });
        }

        public void TriggerEvent(Guid microService, Guid configuration, string name, string primaryKey, string topic, string arguments)
        {
            Connection.Post(Connection.CreateUrl(Controller, "EnqueueEvent"), new
            {
                microService,
                handler = configuration,
                name,
                primaryKey,
                topic,
                arguments
            });
        }
    }
}
