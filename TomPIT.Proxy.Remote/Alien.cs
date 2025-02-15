﻿using System;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class Alien : IAlien
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Mobile { get; set; }
		public string Phone { get; set; }
		public Guid Token { get; set; }
		public Guid Language { get; set; }
		public string TimeZone { get; set; }
		public string ResourceType { get; set; }
		public string ResourcePrimaryKey { get; set; }
	}
}
