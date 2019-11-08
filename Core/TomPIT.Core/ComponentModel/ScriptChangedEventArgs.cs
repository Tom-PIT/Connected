using System;

namespace TomPIT.ComponentModel
{
	public class ScriptChangedEventArgs : EventArgs
	{
		public ScriptChangedEventArgs()
		{

		}
		public ScriptChangedEventArgs(Guid microService, Guid container, Guid sourceCode)
		{
			SourceCode = sourceCode;
			MicroService = microService;
			Container = container;
		}

		public Guid SourceCode { get; set; }
		public Guid Container { get; set; }
		public Guid MicroService { get; set; }
	}
}
