using System;

namespace TomPIT.Security
{
	public enum PermissionValue
	{
		NotSet = 0,
		Allow = 1,
		Deny = 2
	}

	public interface IPermission
	{
		Guid Evidence { get; }
		string Schema { get; }
		string Claim { get; }
		string Descriptor { get; }
		string PrimaryKey { get; }
		PermissionValue Value { get; }
	}
}
