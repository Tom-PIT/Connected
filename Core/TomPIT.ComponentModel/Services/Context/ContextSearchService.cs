using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Search;

namespace TomPIT.Services.Context
{
	internal class ContextSearchService : ContextClient, IContextSearchService
	{
		public ContextSearchService(IExecutionContext context) : base(context)
		{
		}

		public void Add<T>(string catalog, T args)
		{
			Context.Connection().GetService<ISearchService>().Index(ResolveCatalog(catalog), SearchVerb.Add, args);
		}

		public void Remove<T>(string catalog, T args)
		{
			Context.Connection().GetService<ISearchService>().Index(ResolveCatalog(catalog), SearchVerb.Remove, args);
		}

		public ISearchResults Search(string catalog, string term)
		{
			ResolveCatalog(catalog);

			var options = new SearchOptions
			{
				Kind = QueryKind.Term,
				CommandText = term
			};

			options.Catalogs.Add(catalog);

			return Search(options);
		}

		public ISearchResults Search(ISearchOptions options)
		{
			return Context.Connection().GetService<ISearchService>().Search(options);
		}

		public void Update<T>(string catalog, T args)
		{
			Context.Connection().GetService<ISearchService>().Index(ResolveCatalog(catalog), SearchVerb.Update, args);
		}

		private ISearchCatalog ResolveCatalog(string catalog)
		{
			var tokens = catalog.Split("/");

			Context.MicroService.ValidateMicroServiceReference(Context.Connection(), tokens[0]);

			var ms = Context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

			return Context.Connection().GetService<IComponentService>().SelectConfiguration(ms.Token, "SearchCatalog", tokens[1]) as ISearchCatalog;
		}
	}
}
