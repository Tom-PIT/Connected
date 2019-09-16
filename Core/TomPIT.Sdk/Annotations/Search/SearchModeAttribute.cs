using System;
using TomPIT.Search;

namespace TomPIT.Annotations.Search
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SearchModeAttribute : Attribute
	{
		public SearchModeAttribute(SearchMode mode)
		{
			Mode = mode;
		}

		public SearchMode Mode { get; }
	}
}
