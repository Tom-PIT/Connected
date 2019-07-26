using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	internal class ClientSearchResult : SearchResult, IClientSearchResult
	{
		public object Entity { get; set; }
	}
}