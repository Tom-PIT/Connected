using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class SettingManagementController : SysController
	{
		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var resourceGroup = body.Optional("resourceGroup", Guid.Empty);
			var name = body.Required<string>("name");
			var value = body.Optional("value", string.Empty);
			var visible = body.Required<bool>("visible");
			var dataType = body.Required<DataType>("dataType");
			var tags = body.Optional("tags", string.Empty);

			DataModel.Settings.Update(resourceGroup, name, value, visible, dataType, tags);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var resourceGroup = body.Optional("resourceGroup", Guid.Empty);
			var name = body.Required<string>("name");

			DataModel.Settings.Delete(resourceGroup, name);
		}
	}
}
