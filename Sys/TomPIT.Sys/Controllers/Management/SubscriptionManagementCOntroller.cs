using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class SubscriptionManagementController : SysController
	{
		[HttpPost]
		public List<IClientQueueMessage> Dequeue()
		{
			var body = FromBody();

			var count = body.Required<int>("count");
			var resourceGroup = body.Required<string>("resourceGroup");

			var r = new List<IQueueMessage>();

			var rg = DataModel.ResourceGroups.Select(resourceGroup);

			if (rg == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, resourceGroup));

			return DataModel.Subscriptions.DequeueEvents(rg, count);
		}

		[HttpPost]
		public ISubscription Select()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.Subscriptions.Select(token);
		}

		[HttpPost]
		public ISubscriptionEvent SelectEvent()
		{
			var body = FromBody();
			var id = body.Required<Guid>("id");

			return DataModel.Subscriptions.SelectEvent(id);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var resourceGroup = body.Required<Guid>("resourceGroup");

			DataModel.Subscriptions.CompleteEvent(resourceGroup, popReceipt);
		}
	}
}
