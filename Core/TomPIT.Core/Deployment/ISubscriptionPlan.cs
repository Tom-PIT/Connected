using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Deployment
{
	public enum SubscriptionPlanScope
	{
		Private = 1,
		Public = 2
	}
	public interface ISubscriptionPlan
	{
		Guid Token { get; }
		string Company { get; }
		string Name { get; }
		SubscriptionPlanScope Scope { get; }
		float PriceMonth { get; }
		float PriceYear { get; }
		float PriceThreeYear { get; }
	}
}
