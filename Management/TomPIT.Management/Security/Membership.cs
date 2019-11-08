using System;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	internal class Membership : IMembership
	{
		public Guid User { get; set; }
		public Guid Role { get; set; }
	}
}
