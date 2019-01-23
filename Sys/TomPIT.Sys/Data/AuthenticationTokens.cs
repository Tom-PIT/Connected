using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	public class AuthenticationTokens : SynchronizedRepository<IAuthenticationToken, Guid>
	{
		public AuthenticationTokens(IMemoryCache container) : base(container, "authtokens")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Security.AuthenticationTokens.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Security.AuthenticationTokens.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IAuthenticationToken Select(string key)
		{
			return Get(f => string.Compare(f.Key, key, false) == 0);
		}

		public IAuthenticationToken Select(Guid token)
		{
			return Get(token);
		}

		public List<IAuthenticationToken> Query(List<string> resourceGroups)
		{
			var items = new List<Guid>();

			foreach (var i in resourceGroups)
			{
				var rg = DataModel.ResourceGroups.Select(i);

				if (rg == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, i));

				items.Add(rg.Token);
			}

			return Query(items);
		}

		public List<IAuthenticationToken> Query(List<Guid> resourceGroups)
		{
			return Where(f => resourceGroups.Any(t => t == f.ResourceGroup));
		}

		public List<IAuthenticationToken> Query()
		{
			return All();
		}

		public Guid Insert(Guid resourceGroup, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			var rg = DataModel.ResourceGroups.Select(resourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var v = new Validator();

			v.Unique(null, key, nameof(IAuthenticationToken.Key), Query());

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Security.AuthenticationTokens.Insert(rg, u, token, name, description, key, claims, status, validFrom, validTo, startTime, endTime, ipRestrictions);

			Refresh(token);

			NotificationHubs.AuthenticationTokenChanged(token);

			return token;
		}

		public void Update(Guid token, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			var t = Select(token);

			if (t == null)
				throw new SysException(SR.ErrAuthTokenNotFound);

			var v = new Validator();

			v.Unique(t, Key, nameof(IAuthenticationToken.Key), Query());

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.AuthenticationTokens.Update(t, u, name, description, key, claims, status, validFrom, validTo, startTime, endTime, ipRestrictions);

			Refresh(token);
			NotificationHubs.AuthenticationTokenChanged(token);
		}

		public void Delete(Guid token)
		{
			var t = Select(token);

			if (t == null)
				throw new SysException(SR.ErrAuthTokenNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.AuthenticationTokens.Delete(t);

			Remove(token);
			NotificationHubs.AuthenticationTokenRemoved(token);
		}
	}
}
