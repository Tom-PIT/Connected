using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
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
