using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Analysis;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Development
{
	public class ToolsController : SysController
	{
		[HttpGet]
		public List<ITool> Query()
		{
			return DataModel.Tools.Query();
		}

		[HttpPost]
		public ITool Select()
		{
			var body = FromBody();
			var name = body.Required<string>("name");

			return DataModel.Tools.Select(name);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();
			var name = body.Required<string>("name");
			var status = body.Required<ToolStatus>("status");

			DataModel.Tools.Update(name, status);
		}
	}
}
