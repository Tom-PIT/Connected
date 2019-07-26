using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Search;

namespace TomPIT.Services.Context
{
	public interface IContextSearchService
	{
		void Add<T>([CodeAnalysisProvider(ExecutionContext.SearchCatalogProvider)]string catalog, T args);
		void Update<T>([CodeAnalysisProvider(ExecutionContext.SearchCatalogProvider)]string catalog, T args);
		void Remove<T>([CodeAnalysisProvider(ExecutionContext.SearchCatalogProvider)]string catalog, T args);
		IClientSearchResults Search([CodeAnalysisProvider(ExecutionContext.SearchCatalogProvider)]string catalog, string term);
		IClientSearchResults Search(ISearchOptions options);
	}
}
