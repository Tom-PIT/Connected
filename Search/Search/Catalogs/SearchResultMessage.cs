using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Search.Catalogs
{
	internal class SearchResultMessage : ISearchResultMessage
	{
		public SearchResultMessageType Type { get; set; } = SearchResultMessageType.Information;

		public string Text { get; set; }
	}
}
