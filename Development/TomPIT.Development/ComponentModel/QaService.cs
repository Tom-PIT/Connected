using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class QaService : IQaService
	{
		public QaService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid identifier)
		{
			var url = Connection.CreateUrl("Qa", "Delete");
			var e = new JObject
			{
				{"identifier", identifier }
			};

			Connection.Post(url, e);
		}

		public Guid Insert(string title, string description, string api, string body, string tags)
		{
			var url = Connection.CreateUrl("Qa", "Insert");
			var e = new JObject
			{
				{"title", title},
				{"description", description},
				{"api", api},
				{"body", body},
				{"tags", tags}
			};

			return Connection.Post<Guid>(url, e);
		}

		public List<IApiTest> Query()
		{
			var url = Connection.CreateUrl("Qa", "Query");

			return Connection.Get<List<ApiTest>>(url).ToList<IApiTest>();
		}

		public string SelectBody(Guid identifier)
		{
			var url = Connection.CreateUrl("Qa", "SelectBody")
				.AddParameter("identifier", identifier);

			return Connection.Get<string>(url);
		}

		public void Update(Guid identifier, string title, string description, string api, string body, string tags)
		{
			var url = Connection.CreateUrl("Qa", "Update");
			var e = new JObject
			{
				{"title", title},
				{"description", description},
				{"api", api},
				{"body", body},
				{"tags", tags},
				{"identifier", identifier}
			};

			Connection.Post(url, e);
		}
	}
}
