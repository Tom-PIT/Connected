using System;

namespace TomPIT.Cdn
{
	internal class PrintJob : IPrintJob
	{
		public Guid Token { get; set; }

		public PrintJobStatus Status { get; set; }

		public string Error { get; set; }

		public DateTime Created { get; set; }

		public string Provider { get; set; }
		public string Arguments { get; set; }
		public Guid Component { get; set; }
	}
}
