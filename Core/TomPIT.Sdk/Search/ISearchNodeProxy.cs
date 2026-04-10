using TomPIT.ComponentModel.Search;

namespace TomPIT.Search
{
    public interface ISearchNodeProxy
    {
        ISearchResults Search(ISearchOptions options);
    }
}
