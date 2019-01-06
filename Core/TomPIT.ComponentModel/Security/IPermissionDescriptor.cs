using System;
using TomPIT.Net;

namespace TomPIT.Security
{
	public interface IPermissionDescriptor
	{
		string Id { get; }
		IPermissionDescription GetDescription(ISysContext context, Guid evidence);
	}
}
