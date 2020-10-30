using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.IoT;
using TomPIT.SysDb.IoT;

namespace TomPIT.SysDb.Sql.IoT
{
	internal class IoTHandler : IIoTHandler
	{
		public List<IIoTFieldState> SelectState(Guid hub)
		{
			var r = new Reader<IoTFieldState>("tompit.iot_state_sel");

			r.CreateParameter("@hub", hub);

			return r.Execute().ToList<IIoTFieldState>();
		}

		public void UpdateState(Guid hub, List<IIoTFieldStateModifier> fields)
		{
			var a = new JArray();

			foreach (var i in fields)
			{
				var o = new JObject
				{
					{ "hub", hub },
					{ "field", i.Field },
					{ "device", i.Device }
				};

				if (!string.IsNullOrWhiteSpace(i.Value))
					o.Add("value", i.Value);

				a.Add(o);
			}

			var w = new Writer("tompit.iot_state_upd");

			w.CreateParameter("@items", JsonConvert.SerializeObject(a));

			w.Execute();
		}
	}
}
