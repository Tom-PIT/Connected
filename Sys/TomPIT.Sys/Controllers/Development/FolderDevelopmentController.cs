using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Development
{
	public class FolderDevelopmentController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var name = body.Required<string>("name");
			var parent = body.Optional("parent", Guid.Empty);

			return DataModel.Folders.Insert(microService, name, parent);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var name = body.Required<string>("name");
			var token = body.Required<Guid>("token");
			var parent = body.Optional("parent", Guid.Empty);

			DataModel.Folders.Update(microService, token, name, parent);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var f = body.Required<Guid>("token");

			DataModel.Folders.Delete(microService, f);
		}
	}
}
