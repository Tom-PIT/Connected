using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Configuration;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class SettingController : SysController
	{
		[HttpGet]
		public ImmutableList<ISetting> Query()
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
			var nameSpace = body.Optional("nameSpace", string.Empty);

			return DataModel.Settings.Select(name, nameSpace, type, primaryKey);
		}
	}
}
