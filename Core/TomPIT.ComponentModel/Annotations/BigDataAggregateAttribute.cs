using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.BigData;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class BigDataAggregateAttribute : Attribute
	{
		public BigDataAggregateAttribute(AggregateMode mode)
		{
			Mode = mode;
		}

		public AggregateMode Mode { get; }
	}
}
