using System;
using Newtonsoft.Json.Linq;
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

		public void Enqueue<T>(IQueueConfiguration handler, T arguments)
		{
			Enqueue(handler, arguments, TimeSpan.FromDays(2), TimeSpan.Zero);
		}

		public void Enqueue<T>(IQueueConfiguration handler, T arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			var url = Tenant.CreateUrl("Queue", "Enqueue");
			var e = new JObject
			{
				{"component", handler.Component },
				{"expire", expire  },
				{"nextVisible", nextVisible  }
			};

			if (arguments != null)
				e.Add("arguments", SerializationExtensions.Serialize(arguments));

			Tenant.Post(url, e);
		}
	}
}
