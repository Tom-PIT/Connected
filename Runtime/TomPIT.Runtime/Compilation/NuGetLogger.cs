using System.Diagnostics;
using System.Threading.Tasks;
using NuGet.Common;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;

namespace TomPIT.Compilation
{
	internal class NuGetLogger : TenantObject, ILogger
	{
		private const string LogCategory = "NuGet";
		private const string LogSource = "NuGetPackages";
		public NuGetLogger(ITenant tenant) : base(tenant)
		{
		}

		public void Log(LogLevel level, string data)
		{
			Tenant.GetService<ILoggingService>().Write(new LogEntry
			{
				Category = LogCategory,
				Level = ToTraceLevel(level),
				Source = LogSource,
				Message = data
			});
		}

		public void Log(ILogMessage message)
		{
			Tenant.GetService<ILoggingService>().Write(new LogEntry
			{
				Category =  LogCategory,
				Level = ToTraceLevel(message.Level),
				Source = LogSource,
				Message = message.Message
			});
		}

		public async Task LogAsync(LogLevel level, string data)
		{
			Log(level, data);

			await Task.CompletedTask;
		}

		public async Task LogAsync(ILogMessage message)
		{
			Log(message);

			await Task.CompletedTask;
		}

		public void LogDebug(string data)
		{
			Log(LogLevel.Debug, data);
		}

		public void LogError(string data)
		{
			Log(LogLevel.Error, data);
		}

		public void LogInformation(string data)
		{
			Log(LogLevel.Information, data);
		}

		public void LogInformationSummary(string data)
		{
			Log(LogLevel.Information, data);
		}

		public void LogMinimal(string data)
		{
			Log(LogLevel.Minimal, data);
		}

		public void LogVerbose(string data)
		{
			Log(LogLevel.Verbose, data);
		}

		public void LogWarning(string data)
		{
			Log(LogLevel.Warning, data);
		}

		private static TraceLevel ToTraceLevel(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug:
				case LogLevel.Verbose:
					return TraceLevel.Verbose;
				case LogLevel.Information:
				case LogLevel.Minimal:
					return TraceLevel.Info;
				case LogLevel.Warning:
					return TraceLevel.Warning;
				case LogLevel.Error:
					return TraceLevel.Error;
				default:
					return TraceLevel.Off;
			}
		}
	}
}
