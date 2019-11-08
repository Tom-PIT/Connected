using System.Collections.Generic;

namespace TomPIT.Ide.Analysis.Suggestions
{
	public interface ISnippetProvider
	{
		List<ISuggestion> ProvideSnippets(SnippetArgs e);
	}
}
