using System;

namespace TomPIT.SysDb.Development
{
	public interface IMicroServiceMeta
	{
		string Password { get; }
		Guid MicroService { get; }
	}
}
