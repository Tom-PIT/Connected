using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class MetricService : IMetricService
	{
		private Lazy<ConcurrentQueue<IMetric>> _buffer = new Lazy<ConcurrentQueue<IMetric>>();

		public MetricService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Write(IMetric d)
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

				if (!Buffer.TryDequeue(out IMetric m))
					break;

				a.Add(new JObject
				{
					{"component", m.Component },
					{"consumptionIn", m.ConsumptionIn },
					{"consumptionOut", m.ConsumptionOut},
					{"element", m.Element},
					{"end", m.End},
					{"instance", (int)m.Instance},
					{"ip", m.IP.ToString()},
					{"parent", m.Parent},
					{"request", m.Request},
					{"response", m.Response},
					{"result", (int)m.Result},
					{"session", m.Session},
					{"start", m.Start}
				});

				count++;
			}

			if (a.Count == 0)
				return;

			Send(e);
		}

		private void Send(JObject data)
		{
			var u = Connection.CreateUrl("Metric", "Insert");

			Connection.Post(u, data);
		}

		private ConcurrentQueue<IMetric> Buffer { get { return _buffer.Value; } }
	}
}
