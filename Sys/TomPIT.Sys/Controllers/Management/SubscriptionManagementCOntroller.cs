using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
    public class SubscriptionManagementController : SysController
    {
        [HttpPost]
        public ImmutableList<IQueueMessage> Dequeue()
        {
            var body = FromBody();
            var count = body.Required<int>("count");

            return DataModel.Subscriptions.Dequeue(count);
        }

        [HttpPost]
        public ImmutableList<IQueueMessage> DequeueEvents()
        {
            var body = FromBody();
            var count = body.Required<int>("count");

            return DataModel.Subscriptions.DequeueEvents(count);
        }

        [HttpPost]
        public ISubscriptionEvent SelectEvent()
        {
            var body = FromBody();
            var token = body.Required<Guid>("token");

            return DataModel.Subscriptions.SelectEvent(token);
        }

        [HttpPost]
        public void Complete()
        {
            var body = FromBody();

            var popReceipt = body.Required<Guid>("popReceipt");

            DataModel.Subscriptions.Complete(popReceipt);
        }

        [HttpPost]
        public void Ping()
        {
            var body = FromBody();
            var popReceipt = body.Required<Guid>("popReceipt");

            DataModel.Subscriptions.Ping(popReceipt);
        }

        [HttpPost]
        public void CompleteEvent()
        {
            var body = FromBody();
            var popReceipt = body.Required<Guid>("popReceipt");

            DataModel.Subscriptions.CompleteEvent(popReceipt);
        }
    }
}
