using System;

namespace TomPIT.Annotations.Models
{
	public enum StringKind
	{
		NVarChar = 0,
		VarChar = 1,
		Char = 2,
		NChar = 3
	}

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class StringAttribute : Attribute
	{
		public StringKind Kind { get; set; }
	}
}
