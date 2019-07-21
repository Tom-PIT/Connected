using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	internal class SearchResultDescriptor : ISearchResultDescriptor
	{
		public string Title {get;set;}
		public int Lcid {get;set;}
		public string Tags {get;set;}
		public Guid User {get;set;}
		public DateTime Date {get;set;}
		public Guid Catalog {get;set;}
		public string Text {get;set;}
		public string Id {get;set;}
		public string Content {get;set;}
	}
}
