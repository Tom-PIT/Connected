using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Security
{
	public class UserDataModel : CacheRepository<List<IUserData>, Guid>
	{
		public UserDataModel(IMemoryCache container) : base(container, "userdata")
		{
		}

		public List<IUserData> Query(Guid user)
		{
			return Get(user,
				(f) =>
				{
					var u = DataModel.Users.Select(user);

					if (u == null)
						throw new SysException(SR.ErrUserNotFound);

					return Shell.GetService<IDatabaseService>().Proxy.Data.UserData.Query(u);
				});
		}

		public void Update(Guid user, List<IUserData> items)
		{
			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Data.UserData.Update(u, items);

			Remove(user);
			CachingNotifications.UserDataChanged(user);
		}
	}
}