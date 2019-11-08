using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Cdn
{
	public class DataHub : ComponentConfiguration, IDataHubConfiguration
	{
		private ListItems<IDataHubEndpoint> _endpoints = null;
		[Items(DesignUtils.DataHubEndpointItems)]
		public ListItems<IDataHubEndpoint> Endpoints
		{
			get
			{
				if (_endpoints == null)
					_endpoints = new ListItems<IDataHubEndpoint> { Parent = this };

				return _endpoints;
			}
		}
	}
}
