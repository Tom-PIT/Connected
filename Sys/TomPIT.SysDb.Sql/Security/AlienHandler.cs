using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Globalization;
using TomPIT.Security;
using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class AlienHandler : IAlienHandler
	{
		public void Delete(IAlien alien)
		{
			var w = new Writer("tompit.alien_del");

			w.CreateParameter("@id", alien.GetId());

			w.Execute();
		}

		public void Insert(Guid token, string firstName, string lastName, string email, string mobile, string phone, ILanguage language, string timezone)
		{
			var w = new Writer("tompit.alien_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@first_name", firstName, true);
			w.CreateParameter("@last_name", lastName, true);
			w.CreateParameter("@email", email, true);
			w.CreateParameter("@mobile", mobile, true);
			w.CreateParameter("@phone", phone, true);
			w.CreateParameter("@language", language == null ? 0 : language.GetId(), true);
			w.CreateParameter("@timezone", timezone, true);

			w.Execute();
		}

		public List<IAlien> Query()
		{
			return new Reader<Alien>("tompit.alien_que").Execute().ToList<IAlien>();
		}

		public IAlien Select(Guid token)
		{
			var r = new Reader<Alien>("tompit.alien_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(IAlien alien, string firstName, string lastName, string email, string mobile, string phone, ILanguage language, string timezone)
		{
			var w = new Writer("tompit.alien_ud");

			w.CreateParameter("@id", alien.GetId());
			w.CreateParameter("@first_name", firstName, true);
			w.CreateParameter("@last_name", lastName, true);
			w.CreateParameter("@email", email, true);
			w.CreateParameter("@mobile", mobile, true);
			w.CreateParameter("@phone", phone, true);
			w.CreateParameter("@language", language == null ? 0 : language.GetId(), true);
			w.CreateParameter("@timezone", timezone, true);

			w.Execute();
		}
	}
}
