using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Deployment
{
	public enum SubscriptionMode
	{
		Month = 1,
		Year = 2,
		ThreeYear = 3,
		Perpetual = 4
	}

	public interface ISubscription
	{
		Guid Token { get;  }
		Guid Plan { get; }
		Guid AccountKey { get; }
		DateTime Created { get; }
		SubscriptionMode Mode { get; }
		float Price { get; }
		DateTime Start { get; }
		DateTime End { get; }
		string PlanName { get; }
	}
}
