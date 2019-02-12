using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	public interface ICodeAnalysisService
	{
		ImmutableArray<Diagnostic> CheckSyntax<T>(Guid microService, ISourceCode sourceCode);
		ListItems<ISuggestion> Suggestions(IExecutionContext sender, CodeStateArgs e);
		ISignatureInfo Signatures(IExecutionContext sender, CodeStateArgs e);
		IHoverInfo Hover(IExecutionContext sender, CodeStateArgs e);
		ICodeLens CodeLens(IExecutionContext sender, CodeLensArgs e);
		ILocation Range(IExecutionContext sender, CodeStateArgs e);
	}
}
