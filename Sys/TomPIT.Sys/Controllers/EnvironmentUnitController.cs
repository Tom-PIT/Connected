using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class EnvironmentUnitController : SysController
	{
		[HttpGet]
		public List<IEnvironmentUnit> Query()
		{
			return DataModel.EnvironmentUnits.Query();
		}

		[HttpGet]
		public List<IEnvironmentUnit> QueryChildren(Guid parent)
		{
			return DataModel.EnvironmentUnits.Where(parent);
		}

		[HttpGet]
		public IEnvironmentUnit Select(Guid environmentUnit)
		{
			return DataModel.EnvironmentUnits.GetByToken(environmentUnit);
		}
	}
}
