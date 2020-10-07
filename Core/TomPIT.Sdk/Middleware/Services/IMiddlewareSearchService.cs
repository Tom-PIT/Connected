using System.Collections.Generic;
using TomPIT.Search;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareSearchService
	{
		void Add<T>([CIP(CIP.SearchCatalogProvider)] string catalog, T args);
		void Update<T>([CIP(CIP.SearchCatalogProvider)] string catalog, T args);
		void Remove<T>([CIP(CIP.SearchCatalogProvider)] string catalog, T args);
		IClientSearchResults Search([CIP(CIP.SearchCatalogProvider)] string catalog, string term);
		IClientSearchResults Search(ISearchOptions options);
		List<ISearchEntity> GetEntities(IClientSearchResults results);
	}
}
