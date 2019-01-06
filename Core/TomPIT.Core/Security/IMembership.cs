using System;

namespace TomPIT.Security
{
	public interface IMembership
	{
		Guid User { get; }
		Guid Role { get; }
	}
}
