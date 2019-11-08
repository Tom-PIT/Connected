using System;

namespace TomPIT.Annotations.BigData
{
	[AttributeUsage(AttributeTargets.Property)]
	public class BigDataLengthAttribute : Attribute
	{
		public BigDataLengthAttribute(int length)
		{
			Length = length;
		}

		public int Length { get; }
	}
}
