using System.Collections.Generic;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Runtime.Configuration
{
	public interface IClientSys : ISys
	{
		List<IClientSysConnection> Connections { get; }
		List<string> DataProviders { get; }
		List<string> Designers { get; }
		List<string> ResourceGroups { get; }
	}
}
