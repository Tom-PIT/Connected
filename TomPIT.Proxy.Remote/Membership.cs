using System;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class Membership : IMembership
	{
		public Guid User { get; set; }
		public Guid Role { get; set; }
	}
}
