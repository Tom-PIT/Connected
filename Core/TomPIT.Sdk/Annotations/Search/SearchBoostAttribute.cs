using System;

namespace TomPIT.Annotations.Search
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SearchBoostAttribute : Attribute
	{
		public SearchBoostAttribute(float boost)
		{
			Boost = boost;
		}

		public float Boost { get; }
	}
}
