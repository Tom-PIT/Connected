using System;
using TomPIT.Distributed;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ProcessBehaviorAttribute:Attribute
	{
		public ProcessBehavior Behavior { get; set; }
		public string QueueName { get; set; }
	}
}
