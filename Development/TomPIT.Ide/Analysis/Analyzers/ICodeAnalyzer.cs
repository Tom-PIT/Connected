using System.Collections.Generic;
using TomPIT.Ide.Analysis.Hovering;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Ide.Analysis.Signatures;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis.Analyzers
{
	public interface ICodeAnalyzer
	{
		List<ISuggestion> Suggestions(IMiddlewareContext sender, CodeStateArgs e);
		ISignatureInfo Signatures(IMiddlewareContext sender, CodeStateArgs e);
		IHoverInfo Hover(IMiddlewareContext sender, CodeStateArgs e);
		ICodeLensDescriptor[] CodeLens(IMiddlewareContext sender, CodeLensArgs e);
		ILocation Definition(IMiddlewareContext sender, CodeStateArgs e);
	}
}
