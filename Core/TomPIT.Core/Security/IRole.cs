using System;

namespace TomPIT.Security
{
	public enum RoleBehavior
	{
		Implicit = 1,
		Explicit = 2
	}

	public enum RoleVisibility
	{
		Visible = 1,
		Hidden = 2
	}

	public interface IRole
	{
		Guid Token { get; }
		string Name { get; }
		RoleBehavior Behavior { get; }
		RoleVisibility Visibility { get; }
	}
}
