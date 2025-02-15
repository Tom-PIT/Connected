﻿using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Security
{
	public class MembershipModel : SynchronizedRepository<IMembership, string>
	{
		public MembershipModel(IMemoryCache container) : base(container, "membership")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Security.Users.QueryMembership();

			foreach (var i in ds)
				Set(GenerateKey(i.User, i.Role), i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var u = DataModel.Users.Select(new Guid(tokens[0]));

			if (u == null)
			{
				Remove(id);
				return;
			}

			var r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectMembership(u, new Guid(tokens[1]));

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IMembership Select(Guid user, Guid role)
		{
			return Select(GenerateKey(user, role));
		}

		public IMembership Select(string id)
		{
			return Get(id,
				(f) =>
				{
					var tokens = id.Split('.');

					var u = DataModel.Users.Select(new Guid(tokens[0]));

					if (u == null)
						throw new SysException(SR.ErrUserNotFound);

					return Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectMembership(u, new Guid(tokens[1]));
				});
		}

		public ImmutableList<IMembership> Query() { return All(); }
		public ImmutableList<IMembership> QueryForRole(Guid role)
		{
			return Where(f => f.Role == role);
		}

		public ImmutableList<IMembership> Query(Guid user)
		{
			return Where(f => f.User == user);
		}

		public void Insert(Guid user, Guid role)
		{
			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.InsertMembership(u, role);

			Refresh(GenerateKey(user, role));
			CachingNotifications.MembershipAdded(user, role);
		}

		public void Delete(Guid user, Guid role)
		{
			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			if (role == new Guid("{C82BBDAD-E913-4779-8771-981349467860}"))
			{
				var members = QueryForRole(role);

				if (members.Count == 1)
					throw new SysException(SR.ErrRemoveFullControl);
			}

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.DeleteMembership(u, role);

			Remove(GenerateKey(user, role));
			CachingNotifications.MembershipRemoved(user, role);
		}
	}
}