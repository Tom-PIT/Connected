using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Globalization;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class LanguageController : SysController
	{
		[HttpGet]
		public List<ILanguage> Query()
		{
			return DataModel.Languages.Query();
		}

		[HttpGet]
		public ILanguage Select(Guid language)
		{
			return DataModel.Languages.Select(language);
		}
	}
}
