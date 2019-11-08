using System.Collections.Generic;

namespace TomPIT.Cdn.Data
{
	/*
	 * Scaleout is currently not implemented on this service
	 */
	internal interface IDataHubService
	{
		void Connect(string microService, string dataHub, string connectionId, List<DataHubEndpointSubscriber> endpoints);
		void Disconnect(string connectionId);

		void Notify(string endpoint, string arguments);
	}
}
