using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
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
