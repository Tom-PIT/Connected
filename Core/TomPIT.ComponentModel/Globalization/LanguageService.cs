using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;
using TomPIT.Sys.Globalization;

namespace TomPIT.Globalization
{
	internal class LanguageService : ContextCacheRepository<ILanguage, Guid>, ILanguageService
	{
		public LanguageService(ISysContext server) : base(server, "language")
		{

		}

		public List<ILanguage> Query()
		{
			var u = Server.CreateUrl("Language", "Query");

			return Server.Connection.Get<List<Language>>(u).ToList<ILanguage>();
		}

		public ILanguage Select(Guid language)
		{
			return Get(language,
				(f) =>
				{
					var u = Server.CreateUrl("Language", "Select")
					.AddParameter("language", language);

					return Server.Connection.Get<Language>(u);
				});
		}
	}
}
