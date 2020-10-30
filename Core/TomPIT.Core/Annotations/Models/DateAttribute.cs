using System;

namespace TomPIT.Annotations.Models
{
	public enum DateKind
	{
		NotSet = 0,
		Date = 1,
		DateTime = 2,
		DateTime2 = 3,
		SmallDateTime = 4,
		Time = 5
	}
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class DateAttribute : Attribute
	{
		public DateKind Kind { get; set; }
		public int Precision { get; set; }

	}
}
