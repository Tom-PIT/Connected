using TomPIT.ComponentModel.Search;
using TomPIT.Search.Catalogs;
using TomPIT.Search.Indexing;

namespace TomPIT.Search
{
    internal class SearchNodeProxyService : ISearchNodeProxy
    {
        public ISearchResults Search(ISearchOptions options)
        {
            var transaction = new SearchTransaction();

            transaction.Search((ICatalogSearchOptions)options);

            return transaction.Results;
        }
    }
}
