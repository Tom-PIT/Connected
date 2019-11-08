using System;
using System.Collections.Generic;

namespace TomPIT.Globalization
{
	public interface ILanguageService
	{
		ILanguage Select(Guid language);
		ILanguage Select(int lcid);
		List<ILanguage> Query();
	}
}
