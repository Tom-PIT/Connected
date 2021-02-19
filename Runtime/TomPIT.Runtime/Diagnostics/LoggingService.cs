using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Diagnostics
{
	internal class LoggingService : TenantObject, ILoggingService
	{
		private Lazy<ConcurrentQueue<ILogEntry>> _buffer = new Lazy<ConcurrentQueue<ILogEntry>>();
		private readonly bool _dumpEnabled;
		private Lazy<ConcurrentQueue<DumpRecord>> _dumpBuffer = new Lazy<ConcurrentQueue<DumpRecord>>();

		public LoggingService(ITenant tenant) : base(tenant)
		{
			_dumpEnabled = Shell.GetConfiguration<IClientSys>().Diagnostics.DumpEnabled;

		}

		private bool DumpEnabled => _dumpEnabled;
		private ConcurrentQueue<DumpRecord> DumpBuffer => _dumpBuffer.Value;

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
			if (!DumpEnabled)
				return;

			var sb = new StringBuilder();

			while(DumpBuffer.TryDequeue(out DumpRecord record))
				sb.AppendLine($"{record.Created} - {record.Text}");

			if (sb.Length == 0)
				return;

			var fileName = $"dump_{DateTimeOffset.UtcNow.Year}_{DateTimeOffset.UtcNow.Month}_{DateTimeOffset.UtcNow.Day}_{DateTimeOffset.UtcNow.Hour}.txt";
			var directory = Path.Combine(Tenant.GetService<IRuntimeService>().ContentRoot, "logs");
			var path = Path.Combine(directory, fileName);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllText(path, sb.ToString());
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

		private void Send(JObject data)
		{
			var u = Tenant.CreateUrl("Logging", "Insert");

			Tenant.Post(u, data);
		}

		public void Dump(string text)
		{
			if (!DumpEnabled)
				return;

			DumpBuffer.Enqueue(new DumpRecord
			{
				Created = DateTimeOffset.UtcNow,
				Text = text
			});
		}
		private ConcurrentQueue<ILogEntry> Buffer { get { return _buffer.Value; } }
	}
}
