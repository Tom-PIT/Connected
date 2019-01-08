using dotless.Core.Loggers;
using TomPIT.Diagnostics;

namespace TomPIT.Themes
{
	internal class LessCompileLogger : Logger
	{
		public LessCompileLogger() : base(LogLevel.Error)
		{
		}

		protected override void Log(string message)
		{
			var e = new LogEntry
			{
				Authority = "View",
				//AuthorityId=
				Category = "Themes",
				Message = message
			};

			Instance.GetService<ILoggingService>().Write(e);
		}
	}
}
