using TomPIT.Search;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareSearchService
	{
		void Add<T>([CAP(CAP.SearchCatalogProvider)]string catalog, T args);
		void Update<T>([CAP(CAP.SearchCatalogProvider)]string catalog, T args);
		void Remove<T>([CAP(CAP.SearchCatalogProvider)]string catalog, T args);
		IClientSearchResults Search([CAP(CAP.SearchCatalogProvider)]string catalog, string term);
		IClientSearchResults Search(ISearchOptions options);
	}
}
