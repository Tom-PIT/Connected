using System;
using System.Collections.Generic;

namespace TomPIT.Cdn
{
	public interface IRecipient
	{
		SubscriptionResourceType Type { get; }
		string ResourcePrimaryKey { get; }
		Guid Token { get; }
		List<string> Tags { get; }
	}
}
