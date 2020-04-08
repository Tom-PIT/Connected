using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ISearchEntity
	{
		List<ISearchField> Properties { get; }
	}
}
