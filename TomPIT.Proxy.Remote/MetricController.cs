using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;
using TomPIT.Diagnostics;

namespace TomPIT.Proxy.Remote
{
	internal class MetricController : IMetricController
	{
		private const string Controller = "Metric";
		public MetricController()
		{
			Buffer = new();
		}

		private ConcurrentQueue<IMetric> Buffer { get; }

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
					{"instance", (int)m.Features},
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

			if (!a.Any())
				return;

			Send(e);

		}

		private static void Send(JObject data)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Insert"), data);
		}
	}
}
