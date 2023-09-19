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
		public UserDataService(ITenant tenant) : base(tenant, "userdata")
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

			Instance.SysProxy.UserData.Update(user, data);
			Remove(user);
		}

		private List<IUserData> LoadProfile(Guid user)
		{
			return Get(user,
				(f) =>
				{
					return Instance.SysProxy.UserData.Query(user).ToList();
				});
		}
	}
}
