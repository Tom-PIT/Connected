using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TomPIT.Search
{
	internal class SearchResults : SearchResultsContainer, ISearchResults
	{
		private List<ISearchResult> _items = null;

		[JsonConverter(typeof(SearchResultDescriptorConverter))]
		public List<ISearchResult> Items
		{
			get
			{
				if (_items == null)
					_items = new List<ISearchResult>();

				return _items;
			}
		}
	}
}
