using System;

namespace TomPIT
{
	public enum ExceptionSeverity
	{
		Info = 1,
		Warning = 2,
		Critical = 3
	}

	public class RuntimeException : TomPITException
	{
		public RuntimeException(string message) : base(message)
		{

		}

		public RuntimeException(string source, string message) : base(message)
		{
			Source = source;
		}

		public ExceptionSeverity Severity { get; set; } = ExceptionSeverity.Critical;
		public int EventId { get; set; }
		public long Metric { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }

		public static RuntimeException Info(string source, string message)
		{
			return new RuntimeException(source, message)
			{
				Severity = ExceptionSeverity.Info
			};
		}

		public static RuntimeException Warning(string source, string message)
		{
			return new RuntimeException(source, message)
			{
				Severity = ExceptionSeverity.Warning
			};
		}

		public static RuntimeException Critical(string source, string message)
		{
			return new RuntimeException(source, message)
			{
				Severity = ExceptionSeverity.Critical
			};
		}
	}
}
