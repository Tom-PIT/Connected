using System;

namespace TomPIT.Cdn
{
	public interface ISubscriptionEvent
	{
		Guid Subscription { get; }
		Guid Handler { get; }
		string Topic { get; }
		string PrimaryKey { get; }
		string Name { get; }
		DateTime Created { get; }
		Guid Token { get; }
		string Arguments { get; }
	}
}
