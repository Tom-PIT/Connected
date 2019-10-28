using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Security;

namespace TomPIT.App.Models
{
	public class UserDataModel : AjaxModel
	{
		public object GetData()
		{
			var jo = Body.Required<JObject>("data");

			return Services.Data.User.Select<JToken, JToken>(jo.Required<JToken>("primaryKey"), jo.Optional("topic", string.Empty));
		}

		public object QueryData()
		{
			var jo = Body.Required<JObject>("data");

			return Services.Data.User.Query(jo.Required<string>("topic"));
		}

		public void SetData()
		{
			var data = Body.Property("data");

			if (data.Value is JArray)
				SetArrayData();
			else
				SetObjectData();
		}

		private void SetObjectData()
		{
			var jo = Body.Property("data").Value as JObject;

			Services.Data.User.Update(jo.Required<JToken>("primaryKey"), jo.Optional<JToken>("value", null), jo.Optional("topic", string.Empty));
		}

		private void SetArrayData()
		{
			var a = Body.Property("data").Value as JArray;
			var items = new List<IUserData>();

			foreach (JObject i in a)
				items.Add(Services.Data.User.Create(i.Required<JToken>("primaryKey"), i.Optional<JToken>("value", null), i.Optional("topic", string.Empty)));

			Services.Data.User.Update(items);
		}
	}
}
