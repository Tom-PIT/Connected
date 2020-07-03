using System;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NumericAttribute : Attribute
	{
		public int Percision { get; set; }
		public int Scale { get; set; }
	}
}
