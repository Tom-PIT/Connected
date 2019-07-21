using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Search.Catalogs
{
	internal class SearchResults : ISearchResults
	{
		private List<ISearchResultDescriptor> _items = null;
		private List<ISearchResultMessage> _messages = null;

		public List<ISearchResultDescriptor> Items
		{
			get
			{
				if (_items == null)
					_items = new List<ISearchResultDescriptor>();

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
