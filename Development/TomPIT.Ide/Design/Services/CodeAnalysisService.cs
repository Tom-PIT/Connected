using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	internal class CodeAnalysisService : ICodeAnalysisService
	{
		public CodeAnalysisService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public List<IDiagnostic> CheckSyntax(Guid microService, ISourceCode sourceCode)
		{
			if (sourceCode.TextBlob == Guid.Empty)
				return new List<IDiagnostic>();

			var svc = Connection.GetService<ICompilerService>();
			var script = svc.GetScript(microService, sourceCode);

			return script == null ? new List<IDiagnostic>() : script.Errors;
		}

		public List<IDiagnostic> CheckSyntax<T>(Guid microService, ISourceCode sourceCode)
		{
			if (sourceCode.TextBlob == Guid.Empty)
				return new List<IDiagnostic>();

			var svc = Connection.GetService<ICompilerService>();
			var script = svc.GetScript<T>(microService, sourceCode);

			return script == null ? new List<IDiagnostic>() : script.Errors;
		}

		public ListItems<ISuggestion> Suggestions(IExecutionContext sender, CodeStateArgs e)
		{
			ListItems<ISuggestion> r = null;
			using (var s = new CompilerSuggestionsContext(sender, e))
			{
				r = s.Suggestions;

			}

			GC.Collect();

			return r;
		}

		public ISignatureInfo Signatures(IExecutionContext sender, CodeStateArgs e)
		{
			ISignatureInfo r = null;
			using (var s = new CompilerSignaturesContext(sender, e))
			{
				r = s.Signature;

			}

			GC.Collect();

			return r;
		}

		public IHoverInfo Hover(IExecutionContext sender, CodeStateArgs e)
		{
			IHoverInfo r = null;
			using (var s = new CompilerHoverContext(sender, e))
			{
				r = s.Hover;

			}

			GC.Collect();

			return r;
		}

		public ICodeLens CodeLens(IExecutionContext sender, CodeLensArgs e)
		{
			ICodeLens r = null;
			using (var s = new CompilerCodeLensContext(sender, e))
			{
				r = s.CodeLens;

			}

			GC.Collect();

			return r;
		}

		public ILocation Range(IExecutionContext sender, CodeStateArgs e)
		{
			ILocation r = null;

			using (var s = new CompilerDefinitionContext(sender, e))
			{
				r = s.Location;

			}

			GC.Collect();

			return r;
		}
	}
}