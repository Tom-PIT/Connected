using System;
using System.Reflection;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Exceptions
{
	public class TomPITException : Exception
	{
		private string _message = string.Empty;
		//private static readonly string[] ReplaceSources = new string[] { "tompit.", "system.", "middlewarevalidator" };
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

		public override string Source
		{
			get
			{
				if (string.IsNullOrWhiteSpace(base.Source))
				{
					if (Sender != null)
					{
						return Sender.GetType().Name;
					}
				}

				return base.Source;
			}
			set { base.Source = value; }
		}

		private string ParseMessage(string message, params string[] args)
		{
			if (args == null || args.Length == 0)
			{
				return message;
			}

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

		public static Exception Unwrap(object sender, Exception ex)
		{
			if (ex is TargetInvocationException)
				return UnwrapWithData(sender, ex.InnerException);
			else
				return UnwrapWithData(sender, ex);
		}

		private static Exception UnwrapWithData(object sender, Exception ex)
		{
			if (sender != null)
			{
				var script = ResolveScript(sender);

				if (script != null)
				{
					if (ex.Data.Contains("Script"))
						ex.Data["Script"] = script;
					else
						ex.Data.Add("Script", script);
				}

				var resolutionService = Shell.GetService<IMicroServiceResolutionService>();

				if (resolutionService != null)
				{
					var ms = resolutionService.ResolveMicroService(sender);

					if (ms != null)
					{
						if (ex.Data.Contains("MicroService"))
							ex.Data["MicroService"] = ms;
						else
							ex.Data.Add("MicroService", ms);
					}
				}
			}

			return ex;
		}

		private static string ResolveScript(object sender)
		{
			if (sender == null)
				return null;

			return sender.GetType().ShortName();
		}
	}
}
