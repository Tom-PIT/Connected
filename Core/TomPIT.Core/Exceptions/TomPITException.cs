using System;
using System.Text;

namespace TomPIT.Exceptions
{
	public class TomPITException : Exception
	{
		private string _message = string.Empty;
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
	}
}
