using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data
{
	internal class SubscriptionPlan : ISubscriptionPlan
	{
		public Guid Token {get;set;}

		public string Company {get;set;}

		public string Name {get;set;}

		public SubscriptionPlanScope Scope {get;set;}

		public float PriceMonth {get;set;}

		public float PriceYear {get;set;}

		public float PriceThreeYear {get;set;}
	}
}
