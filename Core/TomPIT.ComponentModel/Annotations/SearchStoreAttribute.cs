using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
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
