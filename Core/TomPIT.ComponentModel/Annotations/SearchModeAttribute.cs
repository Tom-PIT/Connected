using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Search;

namespace TomPIT.Annotations
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
