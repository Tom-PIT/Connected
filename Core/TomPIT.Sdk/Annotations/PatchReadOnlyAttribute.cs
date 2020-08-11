using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class PatchReadOnlyAttribute : Attribute
	{
	}
}
