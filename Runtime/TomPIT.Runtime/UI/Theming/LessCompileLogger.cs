using dotless.Core.Loggers;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT.UI.Theming
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

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(e);
		}
	}
}
