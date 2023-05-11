using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Globalization;

namespace TomPIT.Proxy.Remote
{
	internal class LanguageController : ILanguageController
	{
		private const string Controller = "Language";
		public ImmutableList<ILanguage> Query()
		{
			return Connection.Get<List<Language>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<ILanguage>();
		}

		public ILanguage Select(Guid language)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("language", language);

			return Connection.Get<Language>(u);
		}
	}
}
