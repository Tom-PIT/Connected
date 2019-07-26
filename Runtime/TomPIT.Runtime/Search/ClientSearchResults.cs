using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TomPIT.Search
{
	internal class ClientSearchResults : SearchResultsContainer, IClientSearchResults
	{
		private List<IClientSearchResult> _items = null;

		public List<IClientSearchResult> Items
		{
			get
			{
				if (_items == null)
					_items = new List<IClientSearchResult>();

				return _items;
			}
		}
	}
}
