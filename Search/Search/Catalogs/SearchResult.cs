using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Search.Catalogs
{
	internal class SearchResult : ISearchResult
	{
		public Guid Catalog {get;set;}
		public string Content {get;set;}
		public float Score { get; set; }
		public string Text {get;set;}
		public string Title {get;set;}
	}
}
