﻿using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
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