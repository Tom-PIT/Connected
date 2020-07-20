using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Configuration;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class SettingController : SysController
	{
		[HttpGet]
		public List<ISetting> Query()
		{
			return DataModel.Settings.Query();
		}

		[HttpPost]
		public ISetting Select()
		{
			var body = FromBody();
			var name = body.Required<string>("name");
			var type = body.Optional("type", string.Empty);
			var primaryKey = body.Optional("type", string.Empty);

			return DataModel.Settings.Select(name, type, primaryKey);
		}
	}
}
