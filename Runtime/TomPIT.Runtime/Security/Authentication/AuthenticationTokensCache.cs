using System;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Runtime;

namespace TomPIT.Security.Authentication
{
	internal class AuthenticationTokensCache : SynchronizedClientRepository<IAuthenticationToken, Guid>
	{
		public object FirstOrDefault { get; internal set; }

		public AuthenticationTokensCache(ITenant tenant) : base(tenant, "authtoken")
		{
		}

		protected override void OnInitializing()
		{
			ImmutableList<IAuthenticationToken> items;

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				items = Instance.SysProxy.Security.QueryAuthenticationTokens();
			else
				items = Instance.SysProxy.Security.QueryAuthenticationTokens(Tenant.GetService<IResourceGroupService>().QuerySupported().Select(f => f.Name).ToList());

			foreach (var i in items)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid token)
		{
			var d = Instance.SysProxy.Security.SelectAuthenticationToken(token);

			if (d is not null)
				Set(d.Token, d, TimeSpan.Zero);
		}

		public ImmutableList<IAuthenticationToken> Query(Guid resourceGroup)
		{
			return Where(f => f.ResourceGroup == resourceGroup);
		}

		public IAuthenticationToken Select(InstanceFeatures features)
		{
			var claim = AuthenticationTokenClaim.None;

			if (features.HasFlag(InstanceFeatures.Application))
				claim |= AuthenticationTokenClaim.Application;
			else if (features.HasFlag(InstanceFeatures.Worker))
				claim |= AuthenticationTokenClaim.Worker;
			else if (features.HasFlag(InstanceFeatures.Cdn))
				claim |= AuthenticationTokenClaim.Cdn;
			else if (features.HasFlag(InstanceFeatures.IoT))
				claim |= AuthenticationTokenClaim.IoT;
			else if (features.HasFlag(InstanceFeatures.BigData))
				claim |= AuthenticationTokenClaim.BigData;
			else if (features.HasFlag(InstanceFeatures.Search))
				claim |= AuthenticationTokenClaim.Search;
			else if (features.HasFlag(InstanceFeatures.Rest))
				claim |= AuthenticationTokenClaim.Rest;

			if (claim == AuthenticationTokenClaim.None)
				return null;

			var candidates = Where(f => f.Claims.HasFlag(claim));

			if (!candidates.Any())
				return null;

			foreach (var candidate in candidates)
			{
				if (candidate.Status == AuthenticationTokenStatus.Disabled)
					continue;

				if (!candidate.IsValid(Shell.HttpContext.Request, claim))
					continue;

				return candidate;
			}

			return null;
		}

		public IAuthenticationToken Select(string key)
		{
			return Get(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));
		}

		public void NotifyChanged(Guid id)
		{
			Refresh(id);
		}

		public void NotifyRemoved(Guid id)
		{
			Remove(id);
		}
	}
}
