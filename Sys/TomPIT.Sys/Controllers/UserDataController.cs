using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Security;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Security;

namespace TomPIT.Sys.Controllers
{
	public class UserDataController : SysController
	{
		[HttpPost]
		public List<IUserData> Query()
		{
			var body = FromBody();
			var user = body.Required<Guid>("user");

			return DataModel.UserData.Query(user);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();
			var user = body.Required<Guid>("user");
			var items = body.Required<JArray>("items");
			var col = new List<IUserData>();

			foreach (JObject i in items)
			{
				col.Add(new UserDataItem
				{
					PrimaryKey = i.Required<string>("primaryKey"),
					Topic = i.Optional("topic", string.Empty),
					Value = i.Optional("value", string.Empty)
				});
			}

			DataModel.UserData.Update(user, col);
		}
	}
}
