using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public sealed class PriorityAttribute : Attribute
	{
		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}

		public int Priority { get; }
	}
}
