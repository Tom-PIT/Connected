using System;

namespace TomPIT.Cdn
{
	public interface IPrintSpoolerJob
	{
		Guid Token { get; }
		string Mime { get; }
		string Content { get; }
		string Printer { get; }
	}
}
