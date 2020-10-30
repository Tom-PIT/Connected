using System;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class DefaultAttribute : Attribute
	{
		public object Value { get; set; }
	}
}
