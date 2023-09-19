using System;
using System.Collections.Immutable;
using TomPIT.Globalization;

namespace TomPIT.Proxy
{
	public interface ILanguageController
	{
		ILanguage Select(Guid language);
		ImmutableList<ILanguage> Query();
	}
}
