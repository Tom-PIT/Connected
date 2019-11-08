using System.Collections.Generic;
using TomPIT.Ide.Analysis.Hovering;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Ide.Analysis.Signatures;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis.Analyzers
{
	internal class CSharpAnalyzer : ICodeAnalyzer
	{
		public CSharpAnalyzer()
		{

		}

		public ISignatureInfo Signatures(IMiddlewareContext sender, CodeStateArgs e)
		{
			return sender.Tenant.GetService<ICodeAnalysisService>().Signatures(sender, e);
		}

		public List<ISuggestion> Suggestions(IMiddlewareContext sender, CodeStateArgs e)
		{
			return sender.Tenant.GetService<ICodeAnalysisService>().Suggestions(sender, e);
		}

		public IHoverInfo Hover(IMiddlewareContext sender, CodeStateArgs e)
		{
			return sender.Tenant.GetService<ICodeAnalysisService>().Hover(sender, e);
		}

		public ICodeLensDescriptor[] CodeLens(IMiddlewareContext sender, CodeLensArgs e)
		{
			var r = sender.Tenant.GetService<ICodeAnalysisService>().CodeLens(sender, e);

			return r?.Items.ToArray();
		}

		public ILocation Definition(IMiddlewareContext sender, CodeStateArgs e)
		{
			return sender.Tenant.GetService<ICodeAnalysisService>().Range(sender, e);
		}
	}
}
