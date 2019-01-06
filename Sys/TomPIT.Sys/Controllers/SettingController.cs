using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Configuration;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class SettingController : SysController
	{
		[HttpGet]
		public List<ISetting> Query(Guid resourceGroup)
		{
			return DataModel.Settings.Where(resourceGroup);
		}

		[HttpGet]
		public ISetting Select(Guid resourceGroup, string name)
		{
			return DataModel.Settings.Select(resourceGroup, name);
		}
	}
}
