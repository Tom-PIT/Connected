using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TimezoneHandler : ITimezoneHandler
	{
		public void Delete(ITimezone timezone)
		{
			using var w = new Writer("tompit.big_data_timezone_del");

			w.CreateParameter("@id", timezone.GetId());

			w.Execute();

		}

		public void Insert(Guid token, string name, int offset)
		{
			using var w = new Writer("tompit.big_data_timezone_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@name", name);
			w.CreateParameter("@offset", offset);

			w.Execute();
		}

		public List<ITimezone> Query()
		{
			using var r = new Reader<Timezone>("tompit.big_data_timezone_que");

			return r.Execute().ToList<ITimezone>();
		}

		public ITimezone Select(Guid token)
		{
			using var r = new Reader<Timezone>("tompit.big_data_timezone_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(ITimezone timezone, string name, int offset)
		{
			using var w = new Writer("tompit.big_data_timezone_upd");

			w.CreateParameter("@id", timezone.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@offset", offset);

			w.Execute();
		}
	}
}
