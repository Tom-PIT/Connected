﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.IoT;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class IoTController : SysController
	{
		[HttpPost]
		public void UpdateState()
		{
			var body = FromBody();
			var hub = body.Required<Guid>("hub");
			var items = body.Required<JArray>("fields");
			var fields = new List<IIoTFieldStateModifier>();

			foreach (JObject i in items)
			{
				fields.Add(new IoTFieldState
				{
					Field = i.Required<string>("field"),
					Value = i.Optional("value", string.Empty)
				});
			}

			DataModel.IoTState.UpdateState(hub, fields);
		}

		[HttpPost]
		public List<IIoTFieldState> SelectState()
		{
			var body = FromBody();
			var hub = body.Required<Guid>("hub");

			return DataModel.IoTState.Select(hub);
		}
	}
}