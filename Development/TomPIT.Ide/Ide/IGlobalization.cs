using System;
using System.Collections.Generic;

namespace TomPIT.Ide
{
	public interface IGlobalization
	{
		Guid LanguageToken { get; }
		string Language { get; }
		List<string> Languages { get; }
	}
}
