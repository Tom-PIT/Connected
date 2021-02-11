using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class MicroServiceController : SysController
	{
		[HttpGet]
		public List<IMicroService> Query()
		{
			return DataModel.MicroServices.Query();
		}

		[HttpGet]
		public IMicroService SelectByUrl(string url)
		{
			return DataModel.MicroServices.SelectByUrl(url);
		}

		[HttpGet]
		public IMicroService SelectByToken(Guid microService)
		{
			return DataModel.MicroServices.Select(microService);
		}

		[HttpGet]
		public IMicroService Select(string name)
		{
			return DataModel.MicroServices.Select(name);
		}

		[HttpGet]
		public IMicroServiceString SelectString(Guid microService, Guid language, Guid element, string property)
		{
			return DataModel.MicroServiceStrings.Select(microService, language, element, property);
		}

		[HttpGet]
		public List<IMicroServiceString> QueryStrings(Guid microService, Guid language)
		{
			return DataModel.MicroServiceStrings.Query(microService, language);
		}

		[HttpGet]
		public string SelectMeta(Guid microService)
		{
			var r = DataModel.MicroServicesMeta.Select(microService);

			if (r == null)
				return null;

			return r.Content;
		}
	}
}