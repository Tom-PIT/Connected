using System;

namespace TomPIT.Security
{
	public interface IPermissionSchemaDescriptor
	{
		Guid Id { get; }
		string Title { get; }
		string Avatar { get; }
		string Description { get; }
	}
}
