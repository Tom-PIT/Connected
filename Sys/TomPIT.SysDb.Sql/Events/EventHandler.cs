using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Events;

namespace TomPIT.SysDb.Sql.Events
{
	internal class EventHandler : IEventHandler
	{
		public List<IEventDescriptor> Query()
		{
			using var r = new Reader<EventDescriptor>("tompit.event_que");

			return r.Execute().ToList<IEventDescriptor>();
		}

		public void Update(ImmutableList<IEventDescriptor> events)
		{
			using var w = new LongWriter("tompit.event_upd");
			var items = new JArray();

			foreach (var item in events)
			{
				var jo = new JObject
				{
					{"name", item.Name },
					{"created", item.Created },
					{"identifier", item.Identifier },
					{"service", item.MicroService }
				};

				if (!string.IsNullOrEmpty(item.Arguments))
					jo.Add("arguments", item.Arguments);

				if (!string.IsNullOrEmpty(item.Callback))
					jo.Add("callback", item.Callback);

				items.Add(jo);
			};

			w.CreateParameter("@items", items);

			w.Execute();
		}
	}
}
