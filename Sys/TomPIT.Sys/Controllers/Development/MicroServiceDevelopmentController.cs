using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Sys.Data;
using TomPIT.SysDb.Development;

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

		[HttpPost]
		public void RestoreStrings()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var items = body.Required<JArray>("strings");
			var ds = new List<IMicroServiceRestoreString>();
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			foreach (JObject i in items)
			{
				var lcid = i.Required<int>("lcid");
				var language = DataModel.Languages.Select(lcid);

				if (language == null)
					continue;

				ds.Add(new MicroServiceString
				{
					Element = i.Required<Guid>("element"),
					Language = language,
					MicroService = ms,
					Property = i.Required<string>("property"),
					Value = i.Optional("value", string.Empty)
				});
			}

			DataModel.MicroServiceStrings.Restore(ds);
		}
	}
}
