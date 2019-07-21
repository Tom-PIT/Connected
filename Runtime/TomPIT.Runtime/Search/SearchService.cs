using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Services;

namespace TomPIT.Search
{
	internal class SearchService : ServiceBase, ISearchService
	{
		public SearchService(ISysConnection connection) : base(connection)
		{
		}

		public void Index<T>(ISearchCatalog catalog, SearchVerb verb, T args)
		{
			var u = Connection.CreateUrl("Search", "Index");
			var e = new JObject
			{
				{"microService", ((IConfiguration)catalog).MicroService(Connection) },
				{"catalog", catalog.ComponentName(Connection) }
			};

			var a = new JObject
			{
				{"verb", verb.ToString() }
			};

			if (args != null)
				a.Add("arguments", Types.Serialize(a));

			e.Add("arguments", Types.Serialize(a));

			Connection.Post(u, e);
		}

		public ISearchResults Search(ISearchOptions options)
		{
			var url = Connection.GetService<IInstanceEndpointService>().Url(InstanceType.Search, InstanceVerbs.Get);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceType.Search}, {InstanceVerbs.Post})");

			var u = ServerUrl.Create(url, "Search", "Search");

			return Connection.Post<SearchResults>(u, options);
		}
	}
}
