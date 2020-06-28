using System;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NullableAttribute : Attribute
	{
		public bool IsNullable { get; set; } = true;
	}
}
