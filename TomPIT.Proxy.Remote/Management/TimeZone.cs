using System;
using TomPIT.BigData;

namespace TomPIT.Proxy.Remote.Management
{
	internal class TimeZone : ITimeZone
	{
		public string Name { get; set; }

		public int Offset { get; set; }

		public Guid Token { get; set; }
	}
}
