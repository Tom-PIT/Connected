using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Development
{
	public class FeatureDevelopmentController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var name = body.Required<string>("name");

			return DataModel.Features.Insert(microService, name);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var name = body.Required<string>("name");
			var feature = body.Required<Guid>("feature");

			DataModel.Features.Update(microService, feature, name);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var f = body.Required<Guid>("feature");

			DataModel.Features.Delete(microService, f);
		}
	}
}
