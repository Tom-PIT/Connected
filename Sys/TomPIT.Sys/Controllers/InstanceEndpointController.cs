using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class InstanceEndpointController : SysController
	{
		[HttpGet]
		public IInstanceEndpoint Select(Guid endpoint)
		{
			return DataModel.InstanceEndpoints.GetByToken(endpoint);
		}

		[HttpGet]
		public ImmutableList<IInstanceEndpoint> Query()
		{
			return DataModel.InstanceEndpoints.Query();
		}
	}
}
