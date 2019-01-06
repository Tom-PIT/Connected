using System;

namespace TomPIT.Security
{
	internal class Membership : IMembership
	{
		public Guid User { get; set; }
		public Guid Role { get; set; }
	}
}
