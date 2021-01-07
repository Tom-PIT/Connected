using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.Security;
using TomPIT.SysDb.Data;

namespace TomPIT.SysDb.Sql.Data
{
	internal class UserDataHandler : IUserDataHandler
	{
		public List<IUserData> Query(IUser user)
		{
			using var r = new Reader<UserData>("tompit.user_data_que");

			r.CreateParameter("@user", user.GetId());

			return r.Execute().ToList<IUserData>();
		}

		public void Update(IUser user, List<IUserData> data)
		{
			var a = new JArray();

			foreach (var i in data)
			{
				var item = new JObject
				{
					{ "user", Types.Convert<int>( user.GetId()) },
					{ "primary_key", i.PrimaryKey }
				};

				if (!string.IsNullOrWhiteSpace(i.Topic))
					item.Add("topic", i.Topic);


				if (!string.IsNullOrWhiteSpace(i.Value))
					item.Add("value", i.Value);

				a.Add(item);
			}

			using var w = new Writer("tompit.user_data_mdf");

			w.CreateParameter("@items", a);

			w.Execute();
		}
	}
}
