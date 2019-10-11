using System;
using System.IO;
using TomPIT.Reflection;

namespace TomPIT.Exceptions
{
	public class ScriptException : RuntimeException
	{
		public ScriptException()
		{

		}
		public ScriptException(object sender, Exception inner) : base($"({inner.Message} ({ResolveLine(inner)})", inner)
		{
			ExceptionSender = sender;
			Source = ResolveSource();
			Line = ResolveLine(inner);
		}

		public ScriptException(string source, string message, string line, string path, string microService, Guid component, Guid element) : base(source, message)
		{
			Line = line;
			Path = path;
			Component = component;
			Element = element;
			MicroService = microService;
		}

		private object ExceptionSender { get; }

		public string MicroService { get; protected set; }
		public string Line { get; protected set; }
		public string Path { get; protected set; }
		public override string StackTrace => InnerException?.StackTrace;


		private string ResolveSource()
		{
			string currentLine = null;

			if (InnerException == null || string.IsNullOrWhiteSpace(InnerException.StackTrace))
				return ExceptionSender.GetType().ScriptTypeName();

			using (var r = new StringReader(InnerException.StackTrace))
			{
				while ((currentLine = r.ReadLine()) != null)
				{
					var tokens = currentLine.Split(':');

					if (tokens.Length < 2)
						continue;

					var fileToken = tokens[tokens.Length - 2];

					if (!fileToken.Trim().EndsWith("csx"))
						continue;

					var lineToken = tokens[tokens.Length - 2].Split(' ');

					if (lineToken.Length > 1)
					{
						var path = lineToken[lineToken.Length - 1];

						return path.Substring(0, path.Length - 4);
					}
				}
			}

			return ExceptionSender.GetType().ScriptTypeName();
		}
		private static string ResolveLine(Exception ex)
		{
			string currentLine = null;

			if (ex == null || string.IsNullOrWhiteSpace(ex.StackTrace))
				return 0.ToString();

			using (var r = new StringReader(ex.StackTrace))
			{
				while ((currentLine = r.ReadLine()) != null)
				{
					var tokens = currentLine.Split(':');

					if (tokens.Length < 2)
						continue;

					var fileToken = tokens[tokens.Length - 2];

					if (!fileToken.Trim().EndsWith("csx"))
						continue;

					var lineToken = tokens[tokens.Length - 1].Split(' ');

					if (lineToken.Length > 1)
						return lineToken[1].ToString();
				}
			}

			return 0.ToString();
		}
	}
}
