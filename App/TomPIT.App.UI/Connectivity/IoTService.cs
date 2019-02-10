using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.IoT;

namespace TomPIT.Connectivity
{
	internal class IoTService : ClientRepository<List<IIoTFieldState>, Guid>, IIoTService, IIoTNotification
	{
		public IoTService(ISysConnection connection) : base(connection, "hubdata")
		{

		}

		public void NotifyChanged(object sender, IoTStateChangedArgs e)
		{
			var schema = Get(e.Hub);

			if (schema == null)
				return;

			foreach (var i in e.State)
			{
				if (!(schema.FirstOrDefault(f => string.Compare(f.Field, i.Field, true) == 0) is IoTFieldState field))
				{
					schema.Add(new IoTFieldState
					{
						Field = i.Field,
						Modified = DateTime.UtcNow,
						Value = i.Value
					});
				}
				else
				{
					field.Modified = DateTime.UtcNow;
					field.Value = i.Value;
				}
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
