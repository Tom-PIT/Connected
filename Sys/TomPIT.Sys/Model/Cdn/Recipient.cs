﻿using System;
using TomPIT.Cdn;

namespace TomPIT.Sys.Model.Cdn
{
	internal class Recipient : IRecipient
	{
		public SubscriptionResourceType Type { get; set; } = SubscriptionResourceType.User;
		public string ResourcePrimaryKey { get; set; }

		public Guid Token { get; set; }
	}
}