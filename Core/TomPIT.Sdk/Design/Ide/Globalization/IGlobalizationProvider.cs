using System;
using System.Collections.Generic;

namespace TomPIT.Design.Ide.Globalization
{
	public interface IGlobalizationProvider
	{
		Guid LanguageToken { get; }
		string Language { get; }
		List<string> Languages { get; }
	}
}
