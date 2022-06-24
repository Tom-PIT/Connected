using System;
using System.Collections.Generic;

namespace TomPIT.Cdn
{
	public class Recipient : IRecipient
	{
		public SubscriptionResourceType Type { get; set; } = SubscriptionResourceType.User;
		public string ResourcePrimaryKey { get; set; }

		public Guid Token { get; set; }

		public List<string> Tags {get;set;}
	}
}
