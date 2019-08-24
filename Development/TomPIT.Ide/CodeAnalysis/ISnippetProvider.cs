using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Ide.CodeAnalysis
{
	public interface ISnippetProvider
	{
		List<ISuggestion> ProvideSnippets(SnippetArgs e);
	}
}
