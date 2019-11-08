using System;
using TomPIT.Search;

namespace TomPIT.Annotations.Search
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SearchTermVectorAttribute : Attribute
	{
		public SearchTermVectorAttribute(SearchTermVector vector)
		{
			Vector = vector;
		}

		public SearchTermVector Vector { get; }
	}
}
