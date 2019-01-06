using System;

namespace TomPIT
{
	public enum ApiSeverity
	{
		Info = 1,
		Warning = 2,
		Critical = 3
	}

	public class ApiException : Exception
	{
		public ApiException(string source, string message) : base(message)
		{
			Source = source;
		}

		public ApiSeverity Severity { get; set; } = ApiSeverity.Critical;

		public static ApiException Info(string source, string message)
		{
			return new ApiException(source, message)
			{
				Severity = ApiSeverity.Info
			};
		}

		public static ApiException Warning(string source, string message)
		{
			return new ApiException(source, message)
			{
				Severity = ApiSeverity.Warning
			};
		}

		public static ApiException Critical(string source, string message)
		{
			return new ApiException(source, message)
			{
				Severity = ApiSeverity.Critical
			};
		}
	}
}
