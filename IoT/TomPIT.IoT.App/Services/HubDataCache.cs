using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.IoT.Services
{
	internal class HubDataCache : ClientRepository<List<IIoTFieldState>, Guid>
	{
		private Lazy<ConcurrentQueue<JObject>> _buffer = new Lazy<ConcurrentQueue<JObject>>();

		public HubDataCache(ISysConnection connection) : base(connection, "hubdata")
		{

		}

		public List<IIoTFieldState> Select(Guid hub)
		{
			return Get(hub,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var u = Connection.CreateUrl("IoT", "SelectState");
					var e = new JObject
					{
						{ "hub", hub }
					};

					var r = Connection.Post<List<IoTFieldState>>(u, e).ToList<IIoTFieldState>();

					if (r == null)
						r = new List<IIoTFieldState>();

					return r;
				});
		}

		public JObject Update(Guid hub, List<IIoTFieldStateModifier> fields)
		{
			var e = new JObject
			{
				{"hub", hub }
			};
			var r = new JObject();
			var a = new JArray();

			e.Add("fields", a);

			var schema = Select(hub);

			foreach (var i in fields)
			{
				SynchronizeField(schema, i);

				a.Add(new JObject
				{
					{"field", i.Field },
					{"value", i.Value }
				});

				r.Add(i.Field, i.Value);
			}

			Buffer.Enqueue(e);

			return r;
		}

		public void Flush()
		{
			var u = Connection.CreateUrl("IoT", "UpdateState");

			while (!Buffer.IsEmpty)
			{
				if (!Buffer.TryDequeue(out JObject i))
					break;

				Connection.Post(u, i);
			}
		}

		private ConcurrentQueue<JObject> Buffer { get { return _buffer.Value; } }

		internal void Synchronize(IoTStateChangedArgs e)
		{
			var schema = Get(e.Hub);

			if (schema == null)
				return;

			foreach (var i in e.State)
				SynchronizeField(schema, i);
		}

		private void SynchronizeField(List<IIoTFieldState> schema, IIoTFieldStateModifier modifier)
		{
			if (!(schema.FirstOrDefault(f => string.Compare(f.Field, modifier.Field, true) == 0) is IoTFieldState field))
			{
				schema.Add(new IoTFieldState
				{
					Field = modifier.Field,
					Modified = DateTime.UtcNow,
					Value = modifier.Value
				});
			}
			else
			{
				field.Modified = DateTime.UtcNow;
				field.Value = modifier.Value;
			}
		}
	}
}
