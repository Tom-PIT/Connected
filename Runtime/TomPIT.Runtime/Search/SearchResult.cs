using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	internal class SearchResult : ISearchResult
	{
		public Guid Catalog {get;set;}
		public string Content {get;set;}
		public float Score {get;set;}
		public string Text {get;set;}
		public string Title {get;set;}
	}
}
