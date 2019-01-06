using System;

namespace TomPIT.Security
{
	internal class SystemRole : IRole
	{
		public SystemRole(Guid token, string name, RoleBehavior behavior, RoleVisibility visibility)
		{
			Token = token;
			Name = name;
			Behavior = behavior;
			Visibility = visibility;
		}

		public Guid Token { get; set; }
		public string Name { get; set; }
		public RoleVisibility Visibility { get; set; }
		public RoleBehavior Behavior { get; }
	}
}
