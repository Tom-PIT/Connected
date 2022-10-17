using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Exceptions
{
	public class TomPITException : Exception
	{
		private string _message = string.Empty;
		private string _stackTrace = null;
		private string _source = null;
		private string _scriptPath = string.Empty;
		private List<DiagnosticDescriptor> _diagnostics;
		public TomPITException() { }

		public TomPITException(string source, string message)
		{
			Source = source;
			_message = message;
			Initialize();
		}

		public TomPITException(string message)
		{
			_message = message;
			Initialize();
		}

		public TomPITException(string message, params string[] args)
			 : base(message)
		{
			_message = ParseMessage(message, args);
			Initialize();
		}

		public TomPITException(string message, Exception inner)
			 : base(message, inner)
		{
			_message = message;
			Initialize();
		}

		public override string Message { get { return _message; } }

		public List<DiagnosticDescriptor> DiagnosticsTrace => _diagnostics ??= new List<DiagnosticDescriptor>();
		public object Sender { get; set; }
		public override string StackTrace => _stackTrace;

		public string Script { get; set; }
		public bool Logged { get; set; }
		public string ScriptPath => _scriptPath;
		public int Event { get; set; }

		public override string Source
		{
			get => _source;
			set => base.Source = value;
		}

		private static string ParseMessage(string message, params string[] args)
		{
			if (args == null || args.Length == 0)
				return message;

			var sb = new StringBuilder();

			sb.Append(message);
			sb.Append(" (");

			foreach (string i in args)
			{
				sb.AppendFormat("{0}, ", i);
			}

			sb.Remove(sb.Length - 2, 2);
			sb.Append(')');

			return sb.ToString();
		}

		public static TomPITException Unwrap(Exception ex)
		{
			return UnwrapWithData(null, ex);
		}

		public static TomPITException Unwrap(object sender, Exception ex)
		{
			return UnwrapWithData(sender, ex);
		}

		private static TomPITException UnwrapWithData(object sender, Exception ex)
		{
			if (ex is TomPITException tp)
				return tp;
		
			if (ex is TargetInvocationException target && target.InnerException != null)
				return UnwrapWithData(sender, ex.InnerException);
			
			var result = new TomPITException(ex.Message, ex);

			if (sender != null)
			{
				var script = ResolveScript(sender);

				if (script != null)
				{
					result.Script = script;

					if (result.Data.Contains("Script"))
						result.Data["Script"] = script;
					else
						result.Data.Add("Script", script);
				}

				var resolutionService = Shell.GetService<IMicroServiceResolutionService>();

				if (resolutionService != null)
				{
					var ms = resolutionService.ResolveMicroService(sender);

					if (ms != null)
					{
						if (result.Data.Contains("MicroService"))
							result.Data["MicroService"] = ms;
						else
							result.Data.Add("MicroService", ms);
					}
				}
			}

			return result;
		}

		private void Initialize()
		{
			var stackTrace = ParseStackTrace(this, InnerException == null
				? new StackTrace(true)
				: new StackTrace(InnerException, true), DiagnosticsTrace);

			if (InnerException is TomPITException tp)
				DiagnosticsTrace.AddRange(tp.DiagnosticsTrace);

			_stackTrace = stackTrace.Item1;
			_source = stackTrace.Item2;

			if (string.IsNullOrWhiteSpace(_source))
			{
				if (Sender != null)
					_source = Sender.GetType().ScriptTypeName();
				else
					_source = base.Source;
			}

			if (!string.IsNullOrWhiteSpace(Script))
				_scriptPath = Script;
			else
			{
				_scriptPath = Source;

				if (Data.Contains("MicroService"))
					_scriptPath = $"{((IMicroService)Data["MicroService"]).Name}/{_scriptPath}";
			}
		}

		public static (string, string) ParseStackTrace(Exception ex, StackTrace stackTrace, List<DiagnosticDescriptor> diagnosticTrace)
		{
			var stackTraceString = new StringBuilder();
			var source = string.Empty;

			if (stackTrace.FrameCount == 0)
			{
				stackTraceString = stackTraceString.Append(ex.StackTrace);

				return (stackTraceString.ToString(), source);
			}

			foreach (var frame in stackTrace.GetFrames())
			{
				var isView = IsView(frame);

				if (!IsScript(frame) && !isView)
					continue;

				var method = frame.GetMethod();
				var line = frame.GetFileLineNumber();
				var fileName = frame.GetFileName();

				if (string.IsNullOrEmpty(fileName))
					fileName = "?";
				else if (fileName.EndsWith("cshtml"))
				{
					var tokens = fileName.Replace('\\','/').Split('/');

					fileName = $"{tokens[^2]}/{tokens[^1]}";
				}

				var methodName = isView ? "Render" : method == null ? "?" : method.Name;

				stackTraceString.AppendLine($"{methodName} in {fileName} at line {line}");

				if (method is not null)
				{
					diagnosticTrace.Add(new DiagnosticDescriptor
					{
						Method = method,
						Line = line
					});
				}

				source = fileName;

				if (!string.IsNullOrWhiteSpace(source))
				{
					if (ex.Data.Contains("Script"))
						ex.Data["Script"] = source;
					else
						ex.Data.Add("Script", source);

					var type = method.DeclaringType;

					if (type == null)
						continue;

					var resolutionService = Shell.GetService<IMicroServiceResolutionService>();

					if (resolutionService != null)
					{
						var ms = resolutionService.ResolveMicroService(type);

						if (ms != null)
						{
							if (ex.Data.Contains("MicroService"))
								ex.Data["MicroService"] = ms;
							else
								ex.Data.Add("MicroService", ms);
						}
					}
				}
			}

			if (stackTraceString.Length == 0 && ex.InnerException != null)
				stackTraceString.Append(ex.InnerException.StackTrace);

			return (stackTraceString.ToString(), source);
		}

		private static bool IsScript(StackFrame frame)
		{
			var method = frame.GetMethod();

			if (method == null || method.DeclaringType == null)
				return false;

			return method.DeclaringType.FullName.StartsWith("Submission#");
		}

		private static bool IsView(StackFrame frame)
		{
			var fileName = frame.GetFileName();

			if (string.IsNullOrWhiteSpace(fileName))
				return false;

			return fileName.EndsWith("cshtml");
		}
		private static string ResolveScript(object sender)
		{
			if (sender == null)
				return null;

			return sender.GetType().TryScriptTypeName();
		}

		public override string ToString()
		{
			return $"{Message}{System.Environment.NewLine}{StackTrace}";
		}
	}
}
