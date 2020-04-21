using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Search
{
	internal class SearchService : TenantObject, ISearchService
	{
		public SearchService(ITenant tenant) : base(tenant)
		{
		}

		public void Index<T>(ISearchCatalogConfiguration catalog, SearchVerb verb, T args)
		{
			var u = Tenant.CreateUrl("Search", "Index");
			var e = new JObject
			{
				{"microService", ((IConfiguration)catalog).MicroService() },
				{"catalog", catalog.ComponentName() }
			};

			var a = new JObject
			{
				{"verb", verb.ToString() }
			};

			if (args != null)
				a.Add("arguments", Serializer.Serialize(args));

			e.Add("arguments", Serializer.Serialize(a));

			Tenant.Post(u, e);
		}

		public IClientSearchResults Search(ISearchOptions options)
		{
			var url = Tenant.GetService<IInstanceEndpointService>().Url(InstanceType.Search, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceType.Search}, {InstanceVerbs.Post})");

			var u = ServerUrl.Create(url, "Search", "Search");
			var results = Tenant.Post<SearchResults>(u, options);
			var clientResults = new ClientSearchResults();
			var handlers = new Dictionary<Guid, dynamic>();

			if (results.Messages.Count > 0)
				clientResults.Messages.AddRange(results.Messages);

			clientResults.SearchTime = results.SearchTime;
			clientResults.Total = results.Total;

			foreach (var result in results.Items)
			{
				if (result.Catalog == Guid.Empty)
					continue;

				if (!handlers.ContainsKey(result.Catalog))
				{
					var catalog = Tenant.GetService<IComponentService>().SelectConfiguration(result.Catalog) as ISearchCatalogConfiguration;
					var type = Tenant.GetService<ICompilerService>().ResolveType(((IConfiguration)catalog).MicroService(), catalog, catalog.ComponentName());
					var instance = type.CreateInstance<IMiddlewareComponent>();

					instance.SetContext(catalog.CreateContext());

					dynamic dInstance = instance;

					handlers.Add(result.Catalog, dInstance);
				}

				var handler = handlers[result.Catalog];
				var entity = handler.Search(result.Content);

				clientResults.Items.Add(new ClientSearchResult
				{
					Catalog = result.Catalog,
					Content = result.Content,
					Entity = entity == null ? null : Serializer.Serialize(entity),
					Score = result.Score,
					Text = result.Text,
					Title = result.Title
				});
			}

			return clientResults;
		}
	}
}
