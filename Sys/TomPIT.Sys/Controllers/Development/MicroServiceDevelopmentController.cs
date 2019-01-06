using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Development
{
	public class MicroServiceDevelopmentController : SysController
	{
		[HttpPost]
		public void UpdateMeta()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var meta = body.Required<string>("meta");

			DataModel.MicroServicesMeta.Update(microService, Convert.FromBase64String(meta));
		}

		[HttpPost]
		public void UpdateString()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var language = body.Required<Guid>("language");
			var element = body.Required<Guid>("element");
			var property = body.Required<string>("property");
			var value = body.Required<string>("value");

			DataModel.MicroServiceStrings.Update(microService, language, element, property, value);
		}

		[HttpPost]
		public void DeleteString()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var element = body.Required<Guid>("element");
			var property = body.Required<string>("property");

			DataModel.MicroServiceStrings.Delete(microService, element, property);
		}
	}
}
