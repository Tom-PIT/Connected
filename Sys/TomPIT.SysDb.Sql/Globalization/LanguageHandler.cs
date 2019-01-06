using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Globalization;
using TomPIT.SysDb.Globalization;

namespace TomPIT.SysDb.Sql.Globalization
{
	internal class LanguageHandler : ILanguageHandler
	{
		public void Delete(ILanguage target)
		{
			var w = new Writer("tompit.language_del");

			w.CreateParameter("@id", target.GetId());

			w.Execute();
		}

		public void Insert(Guid token, string name, int lcid, LanguageStatus status, string mappings)
		{
			var w = new Writer("tompit.language_ins");

			w.CreateParameter("@name", name);
			w.CreateParameter("@lcid", lcid);
			w.CreateParameter("@status", status);
			w.CreateParameter("@mappings", mappings, true);
			w.CreateParameter("@token", token);

			w.Execute();
		}

		public List<ILanguage> Query()
		{
			return new Reader<Language>("tompit.language_que").Execute().ToList<ILanguage>();
		}

		public ILanguage Select(Guid token)
		{
			var r = new Reader<Language>("tompit.language_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(ILanguage target, string name, int lcid, LanguageStatus status, string mappings)
		{
			var w = new Writer("tompit.language_upd");

			w.CreateParameter("@id", target.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@lcid", lcid);
			w.CreateParameter("@status", status);
			w.CreateParameter("@mappings", mappings, true);

			w.Execute();
		}
	}
}
