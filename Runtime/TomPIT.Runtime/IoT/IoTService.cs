﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.IoT
{
	internal class IoTService : ClientRepository<List<IIoTFieldState>, Guid>, IIoTService, IIoTServiceNotification
	{
		public IoTService(ISysConnection connection) : base(connection, "iothubdata")
		{
		}

		public void NotifyStateChanged(object sender, IoTStateChangedArgs e)
		{
			Synchronize(e);
		}

		private void Synchronize(IoTStateChangedArgs e)
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

		public List<IIoTFieldState> SelectState(Guid hub)
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
	}
}