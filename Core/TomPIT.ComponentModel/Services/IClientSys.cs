using System.Collections.Generic;

namespace TomPIT.Services
{
	public interface IClientSys : ISys
	{
		List<IClientSysConnection> Connections { get; }
		List<string> DataProviders { get; }
		List<string> Designers { get; }
		List<string> ResourceGroups { get; }
	}
}
