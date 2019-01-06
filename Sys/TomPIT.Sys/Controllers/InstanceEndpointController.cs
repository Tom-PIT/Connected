using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Sys.Data;

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
		public List<IInstanceEndpoint> Query()
		{
			return DataModel.InstanceEndpoints.Query();
		}
	}
}
