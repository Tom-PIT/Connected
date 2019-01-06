using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Globalization
{
	internal class LanguageService : ClientRepository<ILanguage, Guid>, ILanguageService
	{
		public LanguageService(ISysConnection connection) : base(connection, "language")
		{

		}

		public List<ILanguage> Query()
		{
			var u = Connection.CreateUrl("Language", "Query");

			return Connection.Get<List<Language>>(u).ToList<ILanguage>();
		}

		public ILanguage Select(Guid language)
		{
			return Get(language,
				(f) =>
				{
					var u = Connection.CreateUrl("Language", "Select")
					.AddParameter("language", language);

					return Connection.Get<Language>(u);
				});
		}
	}
}
