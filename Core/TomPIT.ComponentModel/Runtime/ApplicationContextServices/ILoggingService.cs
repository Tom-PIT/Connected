namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface ILoggingService
	{
		void Error(string category, string source, string message);
		void Warning(string category, string source, string message);
		void Info(string category, string source, string message);

		void Console(string message);
	}
}
