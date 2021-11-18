using System;

namespace TomPIT.Cdn
{
	public interface IPrintSpoolerJob
	{
		Guid Token { get; }
		Guid? Identity { get; }
		string Mime { get; }
		string Content { get; }
		string Printer { get; }
	}
}
