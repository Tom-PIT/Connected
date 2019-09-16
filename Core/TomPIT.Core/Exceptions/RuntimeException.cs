using System;

namespace TomPIT.Exceptions
{
	public enum ExceptionSeverity
	{
		Info = 1,
		Warning = 2,
		Critical = 3
	}

	public class RuntimeException : TomPITException
	{
		private string _stackTrace = null;
		public RuntimeException(string message) : base(message)
		{

		}

		public RuntimeException(string source, string message, Exception inner) : base(message, inner)
		{
			Source = source;
		}

		public RuntimeException(string message, Exception inner) : base(message, inner)
		{

		}

		public RuntimeException(string source, string message) : base(message)
		{
			Source = source;
		}

		public RuntimeException(string source, string message, string stackTrace) : base(message)
		{
			Source = source;
			_stackTrace = stackTrace;
		}

		public ExceptionSeverity Severity { get; set; } = ExceptionSeverity.Critical;
		public int EventId { get; set; }
		public Guid Metric { get; set; }
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

		public override string StackTrace
		{
			get { return _stackTrace == null ? base.StackTrace : _stackTrace; }
		}

	}
}
