using System;
using TomPIT.Development;

namespace TomPIT.Ide.Analysis.Diagnostics
{
	public class DevelopmentError : IDevelopmentError
	{
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public DevelopmentSeverity Severity { get; set; }
		public string Message { get; set; }
		public int Code { get; set; }
		public ErrorCategory Category { get; set; }
		public Guid Identifier { get; set; }
	}
}
