using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TomPIT.Search
{
	internal class SearchResultsContainer : ISearchResultsContainer
	{
		private List<ISearchResultMessage> _messages = null;

		[JsonConverter(typeof(SearchResultMessageConverter))]
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
