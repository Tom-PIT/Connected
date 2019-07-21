using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Search;

namespace TomPIT.Annotations
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
