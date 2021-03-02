using System;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.Sys.Model.Search
{
	public class SysSearchModel
	{
		private const string Queue = "syssearch";
		public void Enqueue(Guid component)
		{
			var message = new JObject
			{
				{ "verb","delete"},
				{ "component",component}
			};

			DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);
		}

		public void Enqueue(Guid component, Guid element)
		{
			var message = new JObject
			{
				{ "verb","delete"},
				{ "component",component},
				{ "element",element}
			};

			DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);
		}
	}
}
