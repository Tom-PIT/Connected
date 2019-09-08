using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TomPIT
{
	public class ScriptException : RuntimeException
	{
		public ScriptException(object sender, Exception inner): base($"({inner.Message} ({ResolveLine(inner)})", inner)
		{
			ExceptionSender = sender;
			Source = ResolveSource();
		}

		private object ExceptionSender { get; }
		public override string StackTrace => InnerException?.StackTrace;


		private string ResolveSource()
		{
			string currentLine = null;

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

			using (var r = new StringReader(ex.StackTrace))
			{
				while((currentLine =  r.ReadLine())!= null)
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
