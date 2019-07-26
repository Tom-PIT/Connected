using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface IClientSearchResult:ISearchResult
	{
		object Entity { get; }
	}
}
