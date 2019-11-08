using System;
using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Ide.Dom
{
	public interface IPermissionElement : IDomElement
	{
		List<string> Claims { get; }

		string PrimaryKey { get; }
		IPermissionDescriptor PermissionDescriptor { get; }
		Guid ResourceGroup { get; }
		string PermissionComponent { get; }
		bool SupportsInherit { get; }
	}
}
