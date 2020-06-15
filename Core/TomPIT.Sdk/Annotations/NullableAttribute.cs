using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NullableAttribute : Attribute
	{
		public bool IsNullable { get; set; } = true;
	}
}
