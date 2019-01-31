using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class LoggingService : ILoggingService
	{
		private Lazy<ConcurrentQueue<ILogEntry>> _buffer = new Lazy<ConcurrentQueue<ILogEntry>>();

		public LoggingService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Write(ILogEntry d)
		{
			Buffer.Enqueue(d);
		}

		public void Flush()
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
			var u = Connection.CreateUrl("Logging", "Insert");

			Connection.Post(u, data);
		}

		private ConcurrentQueue<ILogEntry> Buffer { get { return _buffer.Value; } }
	}
}
