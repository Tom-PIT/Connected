using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TomPIT.Diagnostics;
using TomPIT.Runtime;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class LoggingController : ILoggingController
	{
		public LoggingController()
		{
			DumpBuffer = new();
		}

		private ConcurrentQueue<DumpRecord> DumpBuffer { get; }

		public void Dump(string text)
		{
			DumpBuffer.Enqueue(new DumpRecord
			{
				Created = DateTimeOffset.UtcNow,
				Text = text
			});
		}

		public void Flush()
		{
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

		public void Write(ILogEntry d)
		{
			DataModel.Logging.Insert(new List<ILogEntry> { d });
		}
	}
}
