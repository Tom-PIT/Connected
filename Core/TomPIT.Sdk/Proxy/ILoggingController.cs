using TomPIT.Diagnostics;

namespace TomPIT.Proxy
{
	public interface ILoggingController
	{
		void Write(ILogEntry d);
		void Dump(string text);
		void Flush();
	}
}
