using System;
using System.Collections.Generic;

namespace TomPIT.Security
{
	public interface IPermissionService
	{
		PermissionValue Toggle(string claim, string schema, string evidence, string primaryKey, string permissionDescriptor);
		void Reset(string claim, string schema, string primaryKey);
		void Reset(string primaryKey);
		List<IPermission> Query(string descriptor, string primaryKey);
		List<IPermission> Query(string descriptor, Guid user);
	}
}
