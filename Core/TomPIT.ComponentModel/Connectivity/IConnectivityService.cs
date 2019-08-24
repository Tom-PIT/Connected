using System.Collections.Generic;

namespace TomPIT.Connectivity
{
	public delegate void ConnectionHandler(object sender, SysConnectionArgs e);

	public interface IConnectivityService
	{
		event ConnectionHandler ConnectionInitialized;
		event ConnectionHandler ConnectionInitialize;
		event ConnectionHandler ConnectionInitializing;

		void Insert(string name, string url, string authenticationToken);

		ISysConnection Select(string url);
		ISysConnection Select();

		List<ISysConnectionDescriptor> QueryConnections();
	}
}
