﻿using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	public class IoCEndpointConfiguration : SourceCodeConfiguration, IIoCEndpointConfiguration
	{
		private ListItems<IIoCEndpoint> _endpoints = null;

		[Items(DesignUtils.IoCEndpointItems)]
		public ListItems<IIoCEndpoint> Endpoints
		{
			get
			{
				if (_endpoints == null)
					_endpoints = new ListItems<IIoCEndpoint> { Parent = this };

				return _endpoints;
			}
		}
	}
}