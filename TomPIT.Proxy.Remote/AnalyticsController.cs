using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Analytics;

namespace TomPIT.Proxy.Remote
{
	internal class AnalyticsController : IAnalyticsController
	{
		private const string Controller = "Analytics";

		public AnalyticsController()
		{
			Buffer = new();
		}

		private ConcurrentQueue<JObject> Buffer { get; }

		public void ModifyMru(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity)
		{
			var a = new JArray();

			foreach (var tag in tags)
				a.Add(tag);

			Buffer.Enqueue(new JObject
			{
				{"type", type },
				{"primaryKey", primaryKey },
				{"entity", entity.ToString() },
				{"entityPrimaryKey", entityPrimaryKey },
				{"tags", a },
				{"capacity", capacity }
			});

		}

		public ImmutableList<IMru> QueryMru(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags)
		{
			var a = new JArray();

			foreach (var tag in tags)
				a.Add(tag);

			var u = Connection.CreateUrl(Controller, "QueryMru");
			var e = new JObject
			{
				{"entity", entity.ToString() },
				{"entityPrimaryKey", entityPrimaryKey },
				{"tags", a }
			};

			return Connection.Post<List<Mru>>(u, e).ToImmutableList<IMru>();
		}

		public void Flush()
		{
			var u = Connection.CreateUrl(Controller, "ModifyMru");

			while (!Buffer.IsEmpty)
			{
				if (!Buffer.TryDequeue(out JObject o))
					break;

				Connection.Post(u, o);
			}
		}
	}
}
