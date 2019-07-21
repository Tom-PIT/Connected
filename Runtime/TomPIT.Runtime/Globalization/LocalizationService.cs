using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Globalization
{
	internal class LocalizationService : ServiceBase, ILocalizationService
	{
		public LocalizationService(ISysConnection connection) : base(connection)
		{
			Cache = new StringsCache(Connection);
		}

		public string GetString(string microService, string stringTable, string key, int lcid, bool throwException)
		{
			return Cache.GetString(microService, stringTable, key, lcid, throwException);
		}

		private StringsCache Cache { get; }
	}
}
