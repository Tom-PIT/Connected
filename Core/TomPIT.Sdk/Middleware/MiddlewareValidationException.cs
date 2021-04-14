using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.Middleware
{
	public class MiddlewareValidationException : ValidationException
	{
		private string _source = null;
		private string _stackTrace = null;
		private List<DiagnosticDescriptor> _diagnosticTrace;

		public MiddlewareValidationException(object instance, List<ValidationResult> results)
		{
			Instance = instance;
			Results = results;
			var sb = new StringBuilder();

			foreach (var result in results)
			{
				if (result != null)
					sb.AppendLine(result.ErrorMessage);
			}

			Message = sb.ToString();

			_source = instance?.GetType().ScriptTypeName();

			Initialize();
		}

		public MiddlewareValidationException(object instance, string message) : this(instance, message, null)
		{
		}
		public MiddlewareValidationException(object instance, string message, MiddlewareValidationException inner)
		{
			_source = instance?.GetType().ScriptTypeName();
			Instance = instance;
			Message = message;

			if (inner != null)
			{
				if (Results == null)
					Results = new List<ValidationResult>();

				if (inner.Results != null)
					Results.AddRange(inner.Results);
				else
					Results.Add(new MiddlewareValidationResult(inner.Instance, inner.Message));
			}

			Initialize();
		}

		public object Instance { get; }
		public override string Message { get; }
		public List<ValidationResult> Results { get; }
		public override string Source { get => _source ??= base.Source; set => _source = value; }
		public List<DiagnosticDescriptor> DiagnosticsTrace => _diagnosticTrace ??= new List<DiagnosticDescriptor>();
		public bool Logged { get; set; }
		public override string StackTrace => _stackTrace;

		public void Initialize()
		{
			var stackTrace = TomPITException.ParseStackTrace(this, InnerException == null
				? new StackTrace(true)
				: new StackTrace(InnerException, true), DiagnosticsTrace);

			_stackTrace = stackTrace.Item1;
			_source = stackTrace.Item2;
		}

		public override string ToString()
		{
			var resultsString = new StringBuilder();

			resultsString.AppendLine(Message);
			resultsString.AppendLine(StackTrace);

			if (Results != null && Results.Any())
			{
				foreach (var result in Results)
				{
					if (result is MiddlewareValidationResult mvr)
						resultsString.AppendLine($"{mvr.Instance?.GetType().ScriptTypeName()} - {result}");
					else
						resultsString.AppendLine(result.ToString());
				}
			}

			return resultsString.ToString();
		}
	}
}
