using System;

namespace TomPIT.Security
{
	public interface IPermissionDescription
	{
		Guid Id { get; }
		string Title { get; }
	}
}
