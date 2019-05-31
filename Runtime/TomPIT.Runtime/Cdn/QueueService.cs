using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Workers;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	internal class QueueService : ServiceBase, IQueueService
	{
		public QueueService(ISysConnection connection) : base(connection)
		{
		}

		public void Enqueue(IQueueWorker worker, JObject arguments)
		{
			Enqueue(worker, arguments, TimeSpan.FromDays(2), TimeSpan.Zero);
		}

		public void Enqueue(IQueueWorker worker, JObject arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			var url = Connection.CreateUrl("Queue", "Enqueue");
			var e = new JObject
			{
				{"component", worker.Component },
				{"expire", expire  },
				{"nextVisible", nextVisible  }
			};

			if (arguments != null)
				e.Add("arguments", arguments);

			Connection.Post(url, e);
		}
	}
}
