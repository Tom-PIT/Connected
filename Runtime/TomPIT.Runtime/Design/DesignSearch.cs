using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Search;

namespace TomPIT.Design
{
	internal class DesignSearch : TenantObject, IDesignSearch
	{
		public DesignSearch(ITenant tenant) : base(tenant)
		{
		}

		public void Delete(Guid component, Guid element)
		{
			return; 
			var u = Tenant.CreateUrl("SearchDevelopment", "Delete");
			var e = new JObject
			{
				{"component", component },
				{"element", element }
			};

			Tenant.Post(u, e);
		}

		public void Delete(Guid component)
		{
			return;
			var u = Tenant.CreateUrl("SearchDevelopment", "Delete");
			var e = new JObject
			{
				{"component", component }
			};

			Tenant.Post(u, e);
		}

		public List<ISysSearchResult> Search(ISearchOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
