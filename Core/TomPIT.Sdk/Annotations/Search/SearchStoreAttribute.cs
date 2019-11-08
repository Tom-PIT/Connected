using System;

namespace TomPIT.Annotations.Search
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SearchStoreAttribute : Attribute
	{
		public SearchStoreAttribute(bool enabled)
		{
			Enabled = enabled;
		}

		public bool Enabled { get; }
	}
}
