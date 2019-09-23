using dotless.Core.Loggers;
using TomPIT.Diagnostics;

namespace TomPIT.App.UI.Theming
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
				Category = "Themes",
				Message = message
			};

			Instance.Tenant.GetService<ILoggingService>().Write(e);
		}
	}
}
