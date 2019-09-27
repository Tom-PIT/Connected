using System;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	public class SysSearch
	{
		private const string Queue = "syssearch";
		public void Enqueue(Guid component)
		{
			var message = new JObject
			{
				{ "verb","delete"},
				{ "component",component}
			};

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, Serializer.Serialize(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);
		}

		public void Enqueue(Guid component, Guid element)
		{
			var message = new JObject
			{
				{ "verb","delete"},
				{ "component",component},
				{ "element",element}
			};

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, Serializer.Serialize(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);
		}
	}
}
