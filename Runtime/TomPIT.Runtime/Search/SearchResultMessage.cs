using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	internal class SearchResultMessage : ISearchResultMessage
	{
		public SearchResultMessageType Type {get;set;}

		public string Text {get;set;}
	}
}
