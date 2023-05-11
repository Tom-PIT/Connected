using System;
using System.Collections.Immutable;
using TomPIT.Globalization;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class LanguageController : ILanguageController
	{
		public ImmutableList<ILanguage> Query()
		{
			return DataModel.Languages.Query();
		}

		public ILanguage Select(Guid language)
		{
			return DataModel.Languages.Select(language);
		}
	}
}
