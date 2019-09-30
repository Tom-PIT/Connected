using System;
using System.Collections.Generic;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Ide.Analysis.Definitions;
using TomPIT.Ide.Analysis.Hovering;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Ide.Analysis.Signatures;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis
{
	internal class CodeAnalysisService : TenantObject, ICodeAnalysisService
	{
		private List<ISnippetProvider> _snippetProviders = null;
		public CodeAnalysisService(ITenant tenant) : base(tenant)
		{

		}
		public List<IDiagnostic> CheckSyntax(Guid microService, IText sourceCode)
		{
			if (sourceCode.TextBlob == Guid.Empty)
				return new List<IDiagnostic>();

			var svc = Tenant.GetService<ICompilerService>();
			var script = svc.GetScript(microService, sourceCode);

			return script == null ? new List<IDiagnostic>() : script.Errors;
		}

		public List<IDiagnostic> CheckSyntax<T>(Guid microService, IText sourceCode)
		{
			if (sourceCode.TextBlob == Guid.Empty)
				return new List<IDiagnostic>();

			var svc = Tenant.GetService<ICompilerService>();
			var script = svc.GetScript<T>(microService, sourceCode);

			return script == null ? new List<IDiagnostic>() : script.Errors;
		}

		public ListItems<ISuggestion> Suggestions(IMiddlewareContext sender, CodeStateArgs e)
		{
			ListItems<ISuggestion> r = null;
			using (var s = new CSharpSuggestionsContext(sender, e))
			{
				r = s.Suggestions;

			}

			GC.Collect();

			return r;
		}

		public ISignatureInfo Signatures(IMiddlewareContext sender, CodeStateArgs e)
		{
			ISignatureInfo r = null;
			using (var s = new CSharpSignaturesContext(sender, e))
			{
				r = s.Signature;

			}

			GC.Collect();

			return r;
		}

		public IHoverInfo Hover(IMiddlewareContext sender, CodeStateArgs e)
		{
			IHoverInfo r = null;
			using (var s = new CSharpHoverContext(sender, e))
			{
				r = s.Hover;

			}

			GC.Collect();

			return r;
		}

		public ICodeLens CodeLens(IMiddlewareContext sender, CodeLensArgs e)
		{
			ICodeLens r = null;
			using (var s = new CSharpCodeLensContext(sender, e))
			{
				r = s.CodeLens;

			}

			GC.Collect();

			return r;
		}

		public ILocation Range(IMiddlewareContext sender, CodeStateArgs e)
		{
			ILocation r = null;

			using (var s = new CSharpDefinitionAnalyzer(sender, e))
			{
				r = s.Location;

			}

			GC.Collect();

			return r;
		}

		public void RegisterSnippetProvider(ISnippetProvider provider)
		{
			SnippetProviders.Add(provider);
		}

		public List<ISuggestion> ProvideSnippets(SnippetArgs e)
		{
			var result = new List<ISuggestion>();

			foreach (var provider in SnippetProviders)
			{
				var suggestions = provider.ProvideSnippets(e);

				if (suggestions != null && suggestions.Count > 0)
					result.AddRange(suggestions);
			}

			return result;
		}

		private List<ISnippetProvider> SnippetProviders
		{
			get
			{
				if (_snippetProviders == null)
					_snippetProviders = new List<ISnippetProvider>();

				return _snippetProviders;
			}
		}
	}
}