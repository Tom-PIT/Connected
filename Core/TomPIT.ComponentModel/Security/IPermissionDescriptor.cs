using System;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	public interface IPermissionDescriptor
	{
		string Id { get; }
		IPermissionDescription GetDescription(ISysConnection connection, Guid evidence);
	}
}
