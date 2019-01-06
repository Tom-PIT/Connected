using System;

namespace TomPIT.ComponentModel
{
	public class ScriptChangedEventArgs : EventArgs
	{
		public ScriptChangedEventArgs(Guid microService, Guid sourceCode)
		{
			SourceCode = sourceCode;
			MicroService = microService;
		}

		public Guid SourceCode { get; }
		public Guid MicroService { get; }
	}
}
