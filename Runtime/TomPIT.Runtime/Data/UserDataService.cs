using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Data
{
	internal class UserDataService : ClientRepository<List<IUserData>, Guid>, IUserDataService, IUserNotification
	{
		public UserDataService(ISysConnection connection) : base(connection, "userdata")
		{
		}

		public void NotifyChanged(object sender, UserEventArgs e)
		{
			Remove(e.User);
		}

		public List<IUserData> Query(Guid user, string topic)
		{
			if (user == Guid.Empty)
				return null;

			var profile = LoadProfile(user);

			if (profile == null)
				return null;

			return profile.Where(f => string.Compare(f.Topic, topic, true) == 0).ToList();
		}

		public IUserData Select(Guid user, string primaryKey)
		{
			if (user == Guid.Empty)
				return null;

			var profile = LoadProfile(user);

			if (profile == null)
				return null;

			return profile.FirstOrDefault(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);
		}

		public IUserData Select(Guid user, string primaryKey, string topic)
		{
			if (user == Guid.Empty)
				return null;

			var profile = LoadProfile(user);

			if (profile == null)
				return null;

			return profile.FirstOrDefault(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0 && string.Compare(f.Topic, topic, true) == 0);
		}

		public void Update(Guid user, string primaryKey, string value)
		{
			Update(user, primaryKey, value, null);
		}

		public void Update(Guid user, string primaryKey, string value, string topic)
		{
			Update(user, new List<IUserData>
			{
				new UserData
				{
					PrimaryKey = primaryKey,
					Value = value,
					Topic=topic
				}
			});
		}

		public void Update(Guid user, List<IUserData> data)
		{
			if (user == Guid.Empty)
				return;

			var u = Connection.CreateUrl("UserData", "Update");
			var e = new JObject
			{
				{"user", user }
			};

			var a = new JArray();

			e.Add("items", a);

			foreach (var i in data)
			{
				var item = new JObject
				{
					{"primaryKey", i.PrimaryKey }
				};

				if (!string.IsNullOrWhiteSpace(i.Topic))
					item.Add("topic", i.Topic);

				if (!string.IsNullOrWhiteSpace(i.Value))
					item.Add("value", i.Value);

				a.Add(item);
			};

			Connection.Post(u, e);

			Remove(user);
		}

		private List<IUserData> LoadProfile(Guid user)
		{
			return Get(user,
				(f) =>
				{
					var u = Connection.CreateUrl("UserData", "Query");
					var e = new JObject
					{
						{"user", user }
					};

					return Connection.Post<List<UserData>>(u, e).ToList<IUserData>();
				});
		}
	}
}
