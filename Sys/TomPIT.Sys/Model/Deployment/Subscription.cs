using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Model.Deployment
{
	internal class Subscription : ISubscription
	{
		public Guid Token {get;set;}

		public Guid Plan {get;set;}

		public Guid AccountKey {get;set;}

		public DateTime Created {get;set;}

		public SubscriptionMode Mode {get;set;}

		public float Price {get;set;}

		public DateTime Start {get;set;}

		public DateTime End {get;set;}

		public string PlanName {get;set;}
	}
}
