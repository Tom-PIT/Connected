using System;
using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
	public interface IPermissionHandler
	{
		void Insert(Guid evidence, string schema, string claim, string descriptor, string primaryKey, PermissionValue value);
		void Update(IPermission permission, PermissionValue value);

		void Delete(IPermission permission);

		List<IPermission> Query();

		IPermission Select(Guid evidence, string schema, string claim, string primaryKey);
	}
}
