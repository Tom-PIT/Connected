using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Search.Catalogs
{
	internal class SearchResults : ISearchResults
	{
		private List<ISearchResult> _items = null;
		private List<ISearchResultMessage> _messages = null;

		public List<ISearchResult> Items
		{
			get
			{
				if (_items == null)
					_items = new List<ISearchResult>();

				return _items;
			}
		}

		public List<ISearchResultMessage> Messages
		{
			get
			{
				if (_messages == null)
					_messages = new List<ISearchResultMessage>();

				return _messages;
			}
		}

		public int Total { get; set; }

		public int SearchTime { get; set; }
	}
}
