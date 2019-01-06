using System;
using System.Collections.Generic;
using TomPIT.Globalization;

namespace TomPIT.SysDb.Globalization
{
	public interface ILanguageHandler
	{
		void Insert(Guid token, string name, int lcid, LanguageStatus status, string mappings);
		void Update(ILanguage target, string name, int lcid, LanguageStatus status, string mappings);
		void Delete(ILanguage target);

		ILanguage Select(Guid token);
		List<ILanguage> Query();
	}
}
