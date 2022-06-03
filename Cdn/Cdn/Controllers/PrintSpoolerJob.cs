using System;

namespace TomPIT.Cdn.Controllers
{
	public class PrintSpoolerJob : IPrintSpoolerJob
	{
		public Guid Token { get; set; }

		public Guid? Identity { get; set; }

		public string Mime { get; set; }

		public string Content { get; set; }

		public string Printer { get; set; }

		public int CopyCount { get; set; }

    }
}
