using System;
using System.Collections.Generic;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Ide.Analysis.Hovering;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Ide.Analysis.Signatures;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis
{
	public interface ICodeAnalysisService
	{
		List<IDiagnostic> CheckSyntax<T>(Guid microService, IText sourceCode);
		List<IDiagnostic> CheckSyntax(Guid microService, IText sourceCode);
		ListItems<ISuggestion> Suggestions(IMiddlewareContext sender, CodeStateArgs e);
		ISignatureInfo Signatures(IMiddlewareContext sender, CodeStateArgs e);
		IHoverInfo Hover(IMiddlewareContext sender, CodeStateArgs e);
		ICodeLens CodeLens(IMiddlewareContext sender, CodeLensArgs e);
		ILocation Range(IMiddlewareContext sender, CodeStateArgs e);

		void RegisterSnippetProvider(ISnippetProvider provider);
		List<ISuggestion> ProvideSnippets(SnippetArgs e);
	}
}