using System;
using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{

	public class CompilerScriptArgs : EventArgs
	{
		public CompilerScriptArgs(Guid microService, IText sourceCode)
		{
			MicroService = microService;
			SourceCode = sourceCode;
		}

		public Guid MicroService { get; }
		public IText SourceCode { get; }
	}
}
