using System;
using System.Collections.Generic;
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
			var e = new Dictionary<string, object>
			{
				{ nameof(verb), verb.ToString() },
			};

			if (args is not null)
				e.Add("arguments", Serializer.Serialize(args));

			Instance.SysProxy.Search.Index(catalog.MicroService(), catalog.ComponentName(), Serializer.Serialize(e));
		}

		public IClientSearchResults Search(ISearchOptions options)
		{
			var url = Tenant.GetService<IInstanceEndpointService>().Url(InstanceFeatures.Search, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceFeatures.Search}, {InstanceVerbs.Post})");

			var u = ServerUrl.Create(url, "Search", "Search");

			var args = new HttpRequestArgs().WithCurrentCredentials(MiddlewareDescriptor.Current.User == null ? Guid.Empty : MiddlewareDescriptor.Current.User.AuthenticationToken);
			var results = Tenant.Post<SearchResults>(u, options, args);
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
