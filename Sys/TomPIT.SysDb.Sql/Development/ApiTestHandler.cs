using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class ApiTestHandler : IApiTestHandler
	{
		public void Delete(Guid identifier)
		{
			var w = new Writer("tompit.api_test_del");

			w.CreateParameter("@identifier", identifier);

			w.Execute();
		}

		public void Insert(Guid identifier, string title, string description, string api, string body, string tags)
		{
			var w = new Writer("tompit.api_test_ins");

			w.CreateParameter("@title", title);
			w.CreateParameter("@description", description, true);
			w.CreateParameter("@api", api);
			w.CreateParameter("@body", body, true);
			w.CreateParameter("@tags", tags);
			w.CreateParameter("@identifier", identifier);

			w.Execute();
		}

		public List<IApiTest> Query()
		{
			return new Reader<ApiTest>("tompit.api_test_que").Execute().ToList<IApiTest>();
		}

		public string SelectBody(Guid identifier)
		{
			var r = new Reader<ApiTestBody>("tompit.api_test_sel");

			r.CreateParameter("@identifier", identifier);

			var rs = r.ExecuteSingleRow();

			return rs?.Body;
		}

		public void Update(Guid identifier, string title, string description, string api, string body, string tags)
		{
			var w = new Writer("tompit.api_test_upd");

			w.CreateParameter("@title", title);
			w.CreateParameter("@description", description, true);
			w.CreateParameter("@api", api);
			w.CreateParameter("@body", body, true);
			w.CreateParameter("@identifier", identifier);
			w.CreateParameter("@tags", tags);

			w.Execute();
		}
	}
}
