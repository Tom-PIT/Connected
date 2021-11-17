using System;
using System.Collections.Generic;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Exceptions;
using TomPIT.Search;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareSearchService : MiddlewareObject, IMiddlewareSearchService
	{
		public MiddlewareSearchService(IMiddlewareContext context) : base(context)
		{
		}

		public void Add<T>(string catalog, T args)
		{
			Context.Tenant.GetService<ISearchService>().Index(ResolveCatalog(catalog), SearchVerb.Add, args);
		}

		public List<ISearchEntity> GetEntities(IClientSearchResults results)
		{
			if (results == null)
				return null;

			var result = new List<ISearchEntity>();
			var entities = new Dictionary<Guid, Type>();

			foreach (var item in results.Items)
			{
				Type entity = null;

				if (entities.ContainsKey(item.Catalog))
					entity = entities[item.Catalog];

				if (entity == null)
				{
					entity = CreateCatalog(item.Catalog);

					if (entity == null)
						throw new RuntimeException(nameof(MiddlewareSearchService), $"{SR.ErrCreateSearchCatalog} ({item.Catalog})");

					entities.Add(item.Catalog, entity);

					result.Add(Serializer.Deserialize(item.Entity, entity) as ISearchEntity);
				}
			}

			return result;
		}

		private Type CreateCatalog(Guid configuration)
		{
			var config = Context.Tenant.GetService<IComponentService>().SelectConfiguration(configuration) as ISearchCatalogConfiguration;

			return config.CatalogType();
		}

		public void Remove<T>(string catalog, T args)
		{
			Context.Tenant.GetService<ISearchService>().Index(ResolveCatalog(catalog), SearchVerb.Remove, args);
		}

		public IClientSearchResults Search(string catalog, string term)
		{
			ResolveCatalog(catalog);

			var options = new SearchOptions
			{
				Kind = CommandKind.Term,
				CommandText = term
			};

			options.Catalogs.Add(catalog);

			return Search(options);
		}

		public IClientSearchResults Search(ISearchOptions options)
		{
			return Context.Tenant.GetService<ISearchService>().Search(options);
		}

		public void Update<T>(string catalog, T args)
		{
			Context.Tenant.GetService<ISearchService>().Index(ResolveCatalog(catalog), SearchVerb.Update, args);
		}

		private ISearchCatalogConfiguration ResolveCatalog(string catalog)
		{
			var descriptor = ComponentDescriptor.SearchCatalog(Context, catalog);

			descriptor.Validate();

			return descriptor.Configuration;
		}
    }
}
