using System;
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
		private bool _initialized = false;
		public TomPITException() { }

		public TomPITException(string source, string message)
		{
			Source = source;
			_message = message;
		}

		public TomPITException(string message)
		{
			_message = message;
		}

		public TomPITException(string message, params string[] args)
			 : base(message)
		{
			_message = ParseMessage(message, args);
		}

		public TomPITException(string message, Exception inner)
			 : base(message, inner)
		{
			_message = message;
		}

		public override string Message { get { return _message; } }

		public object Sender { get; set; }

		public string Script { get; set; }
		public bool Logged { get; set; }
		public string ScriptPath
		{
			get
			{
				var script = Script;

				if (string.IsNullOrWhiteSpace(script))
					script = Source;

				if (Data.Contains("MicroService"))
					return $"{((IMicroService)Data["MicroService"]).Name}/{script}";
				else
					return script;
			}
		}
		public override string Source
		{
			get
			{
				Initialize();

				if (string.IsNullOrWhiteSpace(base.Source))
				{
					if (Sender != null)
						return Sender.GetType().ScriptTypeName();
				}

				return base.Source;
			}
			set { base.Source = value; }
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
			sb.Append(")");

			return sb.ToString();
		}

		public int Event { get; set; }

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

		public override string StackTrace
		{
			get
			{
				if(_stackTrace == null)
					Initialize();

				return _stackTrace;
			}
		}
		
		private void Initialize()
		{
			if (_initialized)
				return;

			_initialized = true;

			var sb = new StringBuilder();
			var st = new StackTrace(InnerException ?? this, true);

			if (st.FrameCount == 0)
				_stackTrace = base.StackTrace;

			foreach (var frame in st.GetFrames())
			{
				var method = frame.GetMethod();

				if (method == null || method.DeclaringType == null)
					continue;

				if (!method.DeclaringType.FullName.StartsWith("Submission#"))
					continue;

				var line = frame.GetFileLineNumber();
				var fileName = frame.GetFileName();

				if (string.IsNullOrEmpty(fileName))
					fileName = "?";

				sb.AppendLine($"{method.Name} in {fileName} at line {line}");

				Source = fileName;

				if (!string.IsNullOrWhiteSpace(Source))
				{
					if (Data.Contains("Script"))
						Data["Script"] = Script;
					else
						Data.Add("Script", Script);

					var resolutionService = Shell.GetService<IMicroServiceResolutionService>();

					if (resolutionService != null)
					{
						var ms = resolutionService.ResolveMicroService(method.DeclaringType);

						if (ms != null)
						{
							if (Data.Contains("MicroService"))
								Data["MicroService"] = ms;
							else
								Data.Add("MicroService", ms);
						}
					}
				}
			}

			_stackTrace = sb.ToString();
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
