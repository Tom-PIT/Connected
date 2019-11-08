using System;
using TomPIT.BigData;

namespace TomPIT.Annotations.BigData
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
