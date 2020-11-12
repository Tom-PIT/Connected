using System.Collections.Generic;

namespace TomPIT.Runtime.Configuration
{
	public interface IClientSys : ISys
	{
		List<IClientSysConnection> Connections { get; }
		List<string> DataProviders { get; }
		List<string> Designers { get; }
		List<string> ResourceGroups { get; }
		Platform Platform { get; }
		EnvironmentStage Stage { get; }
	}
}
