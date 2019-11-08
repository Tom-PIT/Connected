using System;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Cdn
{
	internal class QueueService : TenantObject, IQueueService
	{
		public QueueService(ITenant tenant) : base(tenant)
		{
		}

		public void Enqueue<T>(IQueueWorker worker, T arguments)
		{
			Enqueue(worker, arguments, TimeSpan.FromDays(2), TimeSpan.Zero);
		}

		public void Enqueue<T>(IQueueWorker worker, T arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			var url = Tenant.CreateUrl("Queue", "Enqueue");
			var e = new JObject
			{
				{"component", worker.Configuration().Component },
				{"worker", worker.Name },
				{"expire", expire  },
				{"nextVisible", nextVisible  }
			};

			if (arguments != null)
				e.Add("arguments", Serializer.Serialize(arguments));

			Tenant.Post(url, e);
		}
	}
}
