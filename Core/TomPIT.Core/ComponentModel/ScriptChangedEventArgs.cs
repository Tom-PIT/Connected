using System;

namespace TomPIT.ComponentModel
{
	public class ScriptChangedEventArgs : EventArgs
	{
		public ScriptChangedEventArgs(Guid microService, Guid container, Guid sourceCode)
		{
			SourceCode = sourceCode;
			MicroService = microService;
			Container = container;
		}

		public Guid SourceCode { get; }
		public Guid Container { get; }
		public Guid MicroService { get; }
	}
}
