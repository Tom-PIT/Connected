using System.Collections.Generic;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	public interface ICodeCompletionProvider
	{
		List<ISuggestion> Suggestions(IExecutionContext sender, CodeStateArgs e);
		ISignatureInfo Signatures(IExecutionContext sender, CodeStateArgs e);
		IHoverInfo Hover(IExecutionContext sender, CodeStateArgs e);
		ICodeLensDescriptor[] CodeLens(IExecutionContext sender, CodeLensArgs e);
	}
}
