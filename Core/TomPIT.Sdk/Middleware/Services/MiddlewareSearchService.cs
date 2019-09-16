using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Search;

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
			var tokens = catalog.Split("/");

			Context.MicroService.ValidateMicroServiceReference(tokens[0]);

			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			return Context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "SearchCatalog", tokens[1]) as ISearchCatalogConfiguration;
		}
	}
}
