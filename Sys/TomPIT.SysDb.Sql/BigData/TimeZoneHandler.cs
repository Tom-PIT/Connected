using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TimeZoneHandler : ITimeZoneHandler
	{
		public void Delete(ITimeZone timezone)
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

		public List<ITimeZone> Query()
		{
			using var r = new Reader<TimeZone>("tompit.big_data_timezone_que");

			return r.Execute().ToList<ITimeZone>();
		}

		public ITimeZone Select(Guid token)
		{
			using var r = new Reader<TimeZone>("tompit.big_data_timezone_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(ITimeZone timezone, string name, int offset)
		{
			using var w = new Writer("tompit.big_data_timezone_upd");

			w.CreateParameter("@id", timezone.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@offset", offset);

			w.Execute();
		}
	}
}
