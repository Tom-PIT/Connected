using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Globalization;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class LanguageController : SysController
	{
		[HttpGet]
		public ImmutableList<ILanguage> Query()
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
