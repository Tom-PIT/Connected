using System.Collections.Generic;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Sys.Services
{
	public interface IServerSys : ISys
	{
		string Database { get; }
		IServerSysAuthentication Authentication { get; }

		List<string> StorageProviders { get; }
		IServerSysConnectionStrings ConnectionStrings { get; }
	}
}
