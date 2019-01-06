using System;

namespace TomPIT.Security
{
	public interface IUserState
	{
		Guid EnvironmentUnit { get; set; }
	}
}
