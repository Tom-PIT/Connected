using System.Collections.Generic;
using System.Linq;
using TomPIT.Configuration;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Management;

namespace TomPIT.SysDb.Sql.Configuration
{
	internal class SettingHandler : ISettingHandler
	{
		public void Delete(ISetting setting)
		{
			var p = new Writer("tompit.setting_del");

			p.CreateParameter("@id", setting.GetId());

			p.Execute();
		}

		public List<ISetting> Query()
		{
			return new Reader<Setting>("tompit.setting_que").Execute().ToList<ISetting>();
		}

		public ISetting Select(string name, string nameSpace, string type, string primaryKey)
		{
			var p = new Reader<Setting>("tompit.setting_sel");

			p.CreateParameter("@name", name);
			p.CreateParameter("@type", type, true);
			p.CreateParameter("@primary_key", primaryKey, true);
			p.CreateParameter("@namespace", nameSpace, true);

			return p.ExecuteSingleRow();
		}

		public void Insert(string name, string nameSpace, string type, string primaryKey, string value)
		{
			var w = new Writer("tompit.setting_ins");

			w.CreateParameter("@name", name);
			w.CreateParameter("@type", type, true);
			w.CreateParameter("@primary_key", primaryKey, true);
			w.CreateParameter("@value", value, true);
			w.CreateParameter("@namespace", nameSpace, true);

			w.Execute();
		}

		public void Update(ISetting setting, string value)
		{
			var w = new Writer("tompit.setting_upd");

			w.CreateParameter("@id", setting.GetId());
			w.CreateParameter("@value", value, true);

			w.Execute();
		}
	}
}
