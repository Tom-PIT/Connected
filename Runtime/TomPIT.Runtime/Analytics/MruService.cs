using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Analytics
{
	internal class MruService : TenantObject, IMruService
	{
		private Lazy<ConcurrentQueue<JObject>> _buffer = new Lazy<ConcurrentQueue<JObject>>();
		public MruService(ITenant tenant) : base(tenant)
		{

		}

		public void Modify(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity)
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

		internal void Flush()
		{
			var u = Tenant.CreateUrl("Analytics", "ModifyMru");

			while (!Buffer.IsEmpty)
			{
				if (!Buffer.TryDequeue(out JObject o))
					break;

				Tenant.Post(u, o);
			}
		}

		public List<IMru> Query(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags)
		{
			var a = new JArray();

			foreach (var tag in tags)
				a.Add(tag);

			var u = Tenant.CreateUrl("Analytics", "QueryMru");
			var e = new JObject
			{
				{"entity", entity.ToString() },
				{"entityPrimaryKey", entityPrimaryKey },
				{"tags", a }
			};

			return Tenant.Post<List<Mru>>(u, e).ToList<IMru>();
		}

		private ConcurrentQueue<JObject> Buffer { get { return _buffer.Value; } }
	}
}
