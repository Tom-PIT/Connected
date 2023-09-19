using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TomPIT.Diagnostics;
using TomPIT.Runtime;

namespace TomPIT.Proxy.Remote
{
	internal class LoggingController : ILoggingController
	{
		private const string Controller = "Logging";

		public LoggingController()
		{
			DumpBuffer = new();
			Buffer = new();
		}

		public void Dump(string text)
		{
			DumpBuffer.Enqueue(new DumpRecord
			{
				Created = DateTimeOffset.UtcNow,
				Text = text
			});
		}

		private ConcurrentQueue<DumpRecord> DumpBuffer { get; }
		private ConcurrentQueue<ILogEntry> Buffer { get; }

		public void Write(ILogEntry d)
		{
			Buffer.Enqueue(d);
		}

		public void Flush()
		{
			FlushLog();
			FlushDump();
		}

		private void FlushDump()
		{
			var sb = new StringBuilder();

			while (DumpBuffer.TryDequeue(out DumpRecord record))
				sb.AppendLine($"{record.Created} - {record.Text}");

			if (sb.Length == 0)
				return;

			var fileName = $"dump_{DateTimeOffset.UtcNow.Year}_{DateTimeOffset.UtcNow.Month}_{DateTimeOffset.UtcNow.Day}_{DateTimeOffset.UtcNow.Hour}.txt";
			var directory = Path.Combine(Tenant.GetService<IRuntimeService>().ContentRoot, "logs");
			var path = Path.Combine(directory, fileName);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
			using var reader = new StreamReader(ms);
			var lines = new List<string>();

			while (reader.Peek() != -1)
				lines.Add(reader.ReadLine());

			File.AppendAllLines(path, lines);
		}

		private void FlushLog()
		{
			var e = new JObject();
			var a = new JArray();
			var count = 0;

			e.Add("data", a);

			while (!Buffer.IsEmpty)
			{
				if (count > 500)
				{
					Send(e);
					Flush();
					return;
				}

				if (!Buffer.TryDequeue(out ILogEntry log))
					break;

				a.Add(new JObject
				{
					{"category", log.Category },
					{"message", log.Message },
					{"level", log.Level.ToString()},
					{"source", log.Source},
					{"eventId", log.EventId},
					{"component", log.Component},
					{"element", log.Element},
					{"metric", log.Metric},
					{"created", log.Created},
				});

				count++;
			}

			if (a.Count == 0)
				return;

			Send(e);
		}

		private static void Send(JObject data)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Insert"), data);
		}
	}
}
