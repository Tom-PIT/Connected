using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	internal class DesignSearch : TenantObject, IDesignSearch
	{
		public DesignSearch(ITenant tenant) : base(tenant)
		{
		}

		public void Delete(Guid component, Guid element)
		{
			//var u = Tenant.CreateUrl("SearchDevelopment", "Delete");
			//var e = new JObject
			//{
			//	{"component", component },
			//	{"element", element }
			//};

			//Tenant.Post(u, e);
		}

		public void Delete(Guid component)
		{
			//var u = Tenant.CreateUrl("SearchDevelopment", "Delete");
			//var e = new JObject
			//{
			//	{"component", component }
			//};

			//Tenant.Post(u, e);
		}

		public List<ISysSearchResult> Search(ISearchOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
