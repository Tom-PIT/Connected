using System;
using System.Collections.Immutable;

namespace TomPIT.Security
{
	public interface IPermissionService
	{
		PermissionValue Toggle(string claim, string schema, string evidence, string primaryKey, string permissionDescriptor);
		void Reset(string claim, string schema, string primaryKey, string descriptor);
		void Reset(string primaryKey);
		ImmutableList<IPermission> Query(string descriptor, string primaryKey);
		ImmutableList<IPermission> Query(string descriptor, Guid user);
	}
}
