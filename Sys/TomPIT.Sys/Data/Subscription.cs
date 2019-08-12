using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data
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
