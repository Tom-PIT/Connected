using System.Collections.Generic;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Ide.Design.Services
{
	internal class RazorProvider : ICodeCompletionProvider
	{
		public RazorProvider()
		{

		}

		public ISignatureInfo Signatures(IExecutionContext sender, CodeStateArgs e)
		{
			return sender.Connection().GetService<ICodeAnalysisService>().Signatures(sender, e);
		}

		public List<ISuggestion> Suggestions(IExecutionContext sender, CodeStateArgs e)
		{
			return sender.Connection().GetService<ICodeAnalysisService>().Suggestions(sender, e);
		}

		public IHoverInfo Hover(IExecutionContext sender, CodeStateArgs e)
		{
			return sender.Connection().GetService<ICodeAnalysisService>().Hover(sender, e);
		}

		public ICodeLensDescriptor[] CodeLens(IExecutionContext sender, CodeLensArgs e)
		{
			var r = sender.Connection().GetService<ICodeAnalysisService>().CodeLens(sender, e);

			return r?.Items.ToArray();
		}

		public ILocation Definition(IExecutionContext sender, CodeStateArgs e)
		{
			return sender.Connection().GetService<ICodeAnalysisService>().Range(sender, e);
		}
	}
}
