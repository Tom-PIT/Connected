using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ICatalogSearchOptions : ISearchOptions
	{
		List<string> Catalogs { get; }
	}
}
