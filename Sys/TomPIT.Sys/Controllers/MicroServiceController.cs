using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class MicroServiceController : SysController
	{
		[HttpGet]
		public ImmutableList<IMicroService> Query()
		{
			return DataModel.MicroServices.Query();
		}

		[HttpGet]
		public IMicroService SelectByUrl(string url)
		{
			return DataModel.MicroServices.SelectByUrl(url);
		}

		[HttpGet]
		public IMicroService SelectByToken(Guid microService)
		{
			return DataModel.MicroServices.Select(microService);
		}

		[HttpGet]
		public IMicroService Select(string name)
		{
			return DataModel.MicroServices.Select(name);
		}
	}
}