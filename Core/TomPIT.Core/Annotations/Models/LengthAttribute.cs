using System;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class LengthAttribute : Attribute
	{
		public int Length { get; set; }
	}
}
