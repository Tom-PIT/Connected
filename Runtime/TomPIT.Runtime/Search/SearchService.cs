using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
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
				a.Add("arguments", Types.Serialize(args));

			e.Add("arguments", Types.Serialize(a));

			Connection.Post(u, e);
		}

		public IClientSearchResults Search(ISearchOptions options)
		{
			var url = Connection.GetService<IInstanceEndpointService>().Url(InstanceType.Search, InstanceVerbs.Get);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceType.Search}, {InstanceVerbs.Post})");

			var u = ServerUrl.Create(url, "Search", "Search");
			var results = Connection.Post<SearchResults>(u, options);
			var clientResults = new ClientSearchResults();
			var handlers = new Dictionary<Guid, dynamic>();

			if (results.Messages.Count > 0)
				clientResults.Messages.AddRange(results.Messages);

			clientResults.SearchTime = results.SearchTime;
			clientResults.Total = results.Total;

			foreach(var result in results.Items)
			{
				if (result.Catalog == Guid.Empty)
					continue;

				if (!handlers.ContainsKey(result.Catalog))
				{
					var catalog = Connection.GetService<IComponentService>().SelectConfiguration(result.Catalog) as ISearchCatalog;
					var type = Connection.GetService<ICompilerService>().ResolveType(((IConfiguration)catalog).MicroService(Connection), catalog, catalog.ComponentName(Connection));
					dynamic instance = type.CreateInstance<ISearchProcessHandler>(new object[] { catalog.CreateContext() });

					handlers.Add(result.Catalog, instance);
				}

				var handler = handlers[result.Catalog];
				var entity = handler.Deserialize(result.Content);

				clientResults.Items.Add(new ClientSearchResult
				{
					Catalog = result.Catalog,
					Content = result.Content,
					Entity = entity,
					Score = result.Score,
					Text = result.Text,
					Title = result.Title
				});
			}

			return clientResults;
		}
	}
}
