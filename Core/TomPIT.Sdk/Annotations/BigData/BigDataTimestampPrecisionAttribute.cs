using System;
using TomPIT.BigData;

namespace TomPIT.Annotations.BigData
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class BigDataTimestampPrecisionAttribute : Attribute
	{
		public BigDataTimestampPrecisionAttribute(TimestampPrecision precision)
		{
			Precision = precision;
		}

		public TimestampPrecision Precision { get; }
	}
}
