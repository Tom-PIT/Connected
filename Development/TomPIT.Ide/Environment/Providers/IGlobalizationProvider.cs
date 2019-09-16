using System;
using System.Collections.Generic;

namespace TomPIT.Ide.Environment.Providers
{
	public interface IGlobalizationProvider
	{
		Guid LanguageToken { get; }
		string Language { get; }
		List<string> Languages { get; }
	}
}
