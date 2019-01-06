using System.Collections.Generic;

namespace TomPIT.Net
{
	public delegate void ContextRegisteredHandler(object sender, SysContextRegisteredArgs e);

	public interface IConnectivityService
	{
		event ContextRegisteredHandler ContextRegistered;
		event ContextRegisteredHandler ContextInitializing;

		void Insert(string name, string url, string clientKey);

		ISysContext Select(string url);
		ISysContext Select();

		List<ISysConnectionDescriptor> QueryConnections();
	}
}
