using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Events;

namespace TomPIT.SysDb.Sql.Events
{
	internal class EventHandler : IEventHandler
	{
		public void Delete(IEventDescriptor d)
		{
			using var w = new Writer("tompit.event_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public List<IEventDescriptor> Query()
		{
			using var r = new Reader<EventDescriptor>("tompit.event_que");

			return r.Execute().ToList<IEventDescriptor>();
		}

		public IEventDescriptor Select(Guid identifier)
		{
			using var r = new Reader<EventDescriptor>("tompit.event_sel");

			r.CreateParameter("@identifier", identifier);

			return r.ExecuteSingleRow();
		}

		public long Insert(IMicroService microService, string name, Guid identifier, DateTime created, string arguments, string callback)
		{
			using var w = new LongWriter("tompit.event_ins");

			w.CreateParameter("@name", name);
			w.CreateParameter("@identifier", identifier);
			w.CreateParameter("@created", created);
			w.CreateParameter("@arguments", arguments, true);
			w.CreateParameter("@callback", callback, true);
			w.CreateParameter("@service", microService.Token);

			w.Execute();

			return w.Result;
		}
	}
}
