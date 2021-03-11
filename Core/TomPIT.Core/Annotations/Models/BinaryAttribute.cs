using System;

namespace TomPIT.Annotations.Models
{
	public enum BinaryKind
	{
		VarBinary = 0,
		Binary = 1
	}

	[AttributeUsage(AttributeTargets.Property)]

	public sealed class BinaryAttribute : Attribute
	{
		public BinaryKind Kind { get; set; }
	}
}
