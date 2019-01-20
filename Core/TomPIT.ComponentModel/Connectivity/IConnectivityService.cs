using System.Collections.Generic;

namespace TomPIT.Connectivity
{
	public delegate void ConnectionRegisteredHandler(object sender, SysConnectionRegisteredArgs e);

	public interface IConnectivityService
	{
		event ConnectionRegisteredHandler ConnectionRegistered;
		event ConnectionRegisteredHandler ConnectionInitializing;

		void Insert(string name, string url, string authenticationToken);

		ISysConnection Select(string url);
		ISysConnection Select();

		List<ISysConnectionDescriptor> QueryConnections();
	}
}
