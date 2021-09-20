using System;
using System.Collections.Immutable;

namespace TomPIT.Globalization
{
	public interface ILanguageService
	{
		ILanguage Select(Guid language);
		ILanguage Select(int lcid);
		ILanguage Match(string languageString);
		ImmutableList<ILanguage> Query();
	}
}
