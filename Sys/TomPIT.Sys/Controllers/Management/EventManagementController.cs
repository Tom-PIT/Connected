using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Storage;
using TomPIT.Sys.Data;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Controllers.Management
{
	public class EventManagementController : SysController
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

			return DataModel.Events.Dequeue(rg, count);
		}

		[HttpGet]
		public IEventDescriptor Select(Guid id)
		{
			return DataModel.Events.Select(id);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var resourceGroup = body.Required<Guid>("resourceGroup");

			DataModel.Events.Complete(resourceGroup, popReceipt);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var resourceGroup = body.Required<Guid>("resourceGroup");

			DataModel.Events.Ping(resourceGroup, popReceipt);
		}
	}
}
