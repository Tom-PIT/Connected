using System;

namespace TomPIT.Cdn
{
	public enum SubscriptionResourceType
	{
		User = 1,
		Role = 2,
		Alien = 3
	}

	public interface ISubscriber : IRecipient
	{
		Guid Handler { get; }
		string Topic { get; }
		string PrimaryKey { get; }
	}
}
