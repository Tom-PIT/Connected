namespace TomPIT.Search
{
	public class SearchField : ISearchField
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public SearchTermVector TermVector { get; set; } = SearchTermVector.No;

		public SearchMode Mode { get; set; } = SearchMode.Analyzed;

		public float Boost { get; set; }
	}
}
