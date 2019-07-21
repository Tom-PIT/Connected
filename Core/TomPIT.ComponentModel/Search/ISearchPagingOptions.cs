using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchPagingOptions
	{
		int Index { get; }
		int Size { get; }
	}
}
