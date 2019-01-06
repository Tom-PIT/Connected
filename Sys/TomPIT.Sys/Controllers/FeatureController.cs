using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel.Features;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class FeatureController : SysController
	{
		[HttpGet]
		public List<IFeature> Query(Guid microService)
		{
			return DataModel.Features.Query(microService);
		}

		[HttpGet]
		public IFeature SelectByToken(Guid microService, Guid feature)
		{
			return DataModel.Features.Select(microService, feature);
		}

		[HttpGet]
		public IFeature Select(Guid microService, string name)
		{
			return DataModel.Features.Select(microService, name);
		}
	}
}
