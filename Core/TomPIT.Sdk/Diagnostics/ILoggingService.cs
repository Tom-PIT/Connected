namespace TomPIT.Diagnostics
{
	public interface ILoggingService
	{
		void Write(ILogEntry d);
		void Dump(string text);

		bool DumpEnabled { get; }
	}
}
