using System;
using System.Reflection;
using System.Text;
using TomPIT.Reflection;

namespace TomPIT.Exceptions
{
	public class TomPITException : Exception
	{
		private string _message = string.Empty;
		private static readonly string[] ReplaceSources = new string[] { "tompit.", "system." };
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
			{
				var re = ex.InnerException;

				if (sender != null)
					re.Source = ResolveSource(sender, re.Source);

				return re;
			}

			if (sender != null)
				ex.Source = ResolveSource(sender, ex.Source);

			return ex;
		}

		private static string ResolveSource(object sender, string source)
		{
			if (sender == null)
				return source;

			if (string.IsNullOrEmpty(source))
				return sender.GetType().ShortName();
			else
			{
				foreach (var replace in ReplaceSources)
				{
					if (source.ToLowerInvariant().StartsWith(replace))
						return sender.GetType().ShortName();
				}
			}

			return source;
		}
	}
}
