using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Data;

namespace TomPIT.Search
{
	public class SearchEntity : DataEntity
	{
		[SearchStore(true)]
		[SearchMode(SearchMode.NotAnalyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Key { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public int Lcid { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public int TranslationId { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Title { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Content { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Tags { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Author { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.NotAnalyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public DateTime Date { get; set; }
	}
}