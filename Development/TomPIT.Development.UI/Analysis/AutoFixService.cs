using System;
using System.Collections.Generic;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.Development.Analysis
{
	internal class AutoFixService : TenantObject, IAutoFixService
	{
		public AutoFixService(ITenant tenant) : base(tenant)
		{
		}

		public void Complete(Guid popReceipt)
		{
			//var u = Tenant.CreateUrl("DevelopmentErrors", "Complete");
			//var e = new JObject
			//{
			//	{"popReceipt", popReceipt }
			//};

			//Tenant.Post(u, e);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return new();
			//var u = Tenant.CreateUrl("DevelopmentErrors", "Dequeue");
			//var e = new JObject
			//{
			//	{"count", count }
			//};

			//return Tenant.Post<List<QueueMessage>>(u, e).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt)
		{
			//var u = Tenant.CreateUrl("DevelopmentErrors", "Ping");
			//var e = new JObject
			//{
			//	{"popReceipt", popReceipt },
			//	{"nextVisible", 300 }
			//};

			//Tenant.Post(u, e);
		}
	}
}
