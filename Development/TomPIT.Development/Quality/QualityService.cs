using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Development.Quality
{
	internal class QualityService : TenantObject, IQualityService
	{
		public QualityService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid identifier)
		{
			//var url = Tenant.CreateUrl("Qa", "Delete");
			//var e = new JObject
			//{
			//	{"identifier", identifier }
			//};

			//Tenant.Post(url, e);
		}

		public Guid Insert(string title, string description, string api, string body, string tags)
		{
			return Guid.Empty;
			//var url = Tenant.CreateUrl("Qa", "Insert");
			//var e = new JObject
			//{
			//	{"title", title},
			//	{"description", description},
			//	{"api", api},
			//	{"body", body},
			//	{"tags", tags}
			//};

			//return Tenant.Post<Guid>(url, e);
		}

		public List<IApiTest> Query()
		{
			return new();
			//var url = Tenant.CreateUrl("Qa", "Query");

			//return Tenant.Get<List<ApiTest>>(url).ToList<IApiTest>();
		}

		public string SelectBody(Guid identifier)
		{
			return null;
			//var url = Tenant.CreateUrl("Qa", "SelectBody")
			//	.AddParameter("identifier", identifier);

			//return Tenant.Get<string>(url);
		}

		public void Update(Guid identifier, string title, string description, string api, string body, string tags)
		{
			//var url = Tenant.CreateUrl("Qa", "Update");
			//var e = new JObject
			//{
			//	{"title", title},
			//	{"description", description},
			//	{"api", api},
			//	{"body", body},
			//	{"tags", tags},
			//	{"identifier", identifier}
			//};

			//Tenant.Post(url, e);
		}
	}
}
