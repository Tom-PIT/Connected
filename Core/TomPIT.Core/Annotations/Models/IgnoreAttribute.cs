using System;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public sealed class IgnoreAttribute : Attribute
	{
	}
}
