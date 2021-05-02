using System;
using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{

	public class CompilerScriptArgs : EventArgs
	{
		public CompilerScriptArgs(Guid microService, IText sourceCode, bool includeAnalyzers)
		{
			IncludeAnalyzers = includeAnalyzers;
			MicroService = microService;
			SourceCode = sourceCode;
		}

		public bool IncludeAnalyzers { get; }
		public Guid MicroService { get; }
		public IText SourceCode { get; }
	}
}
