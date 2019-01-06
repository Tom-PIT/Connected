using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Security
{
	internal class AuthorizationService : SynchronizedClientRepository<IPermission, string>, IAuthorizationService, IAuthorizationNotification, IMembershipProvider
	{
		private List<IAuthorizationProvider> _providers = null;
		private Lazy<List<IPermissionDescriptor>> _descriptors = new Lazy<List<IPermissionDescriptor>>();
		private Lazy<List<IAuthenticationProvider>> _authProviders = new Lazy<List<IAuthenticationProvider>>();
		private IAuthenticationProvider _defaultAuthenticationProvider = null;

		public AuthorizationService(ISysConnection server) : base(server, "permissions")
		{
			Membership = new MembershipCache(server);

			Providers.Add(new RoleAuthorizationProvider());
			Providers.Add(new UserAuthorizationProvider());
			Providers.Add(new EnvironmentUnitAuthorizationProvider());
			Providers.Add(new PolicyAuthorizationProvider());
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("Security", "QueryPermissions");
			var ds = Connection.Get<List<Permission>>(u).ToList<IPermission>();

			foreach (var i in ds)
				Set(GenerateRandomKey(), i, TimeSpan.Zero);
		}

		public IClientAuthenticationResult Authenticate(string user, string password)
		{
			foreach (var i in AuthenticationProviders)
			{
				var r = i.Authenticate(user, password);

				if (r != null)
					return r;
			}

			return DefaultAuthenticationProvider.Authenticate(user, password);
		}

		public IClientAuthenticationResult Authenticate(string bearerKey)
		{
			foreach (var i in AuthenticationProviders)
			{
				var r = i.Authenticate(bearerKey);

				if (r != null)
					return r;
			}

			return DefaultAuthenticationProvider.Authenticate(bearerKey);
		}

		public IAuthorizationResult Authorize(IExecutionContext context, AuthorizationArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Claim))
				return AuthorizationResult.Fail(AuthorizationResultReason.NoClaim);

			if (string.IsNullOrWhiteSpace(e.PrimaryKey))
				return AuthorizationResult.Fail(AuthorizationResultReason.NoPrimaryKey);

			var permissions = Where(f => string.Compare(f.Claim, e.Claim, true) == 0
				&& string.Compare(f.PrimaryKey, e.PrimaryKey, true) == 0);

			var state = new Dictionary<string, object>();

			foreach (var i in Providers)
			{
				if (i.PreAuthorize(context, e, state) == AuthorizationProviderResult.Success)
					return AuthorizationResult.OK();
			}

			if (permissions.Count == 0)
			{
				switch (e.Schema.Empty)
				{
					case EmptyBehavior.Deny:
						return AuthorizationResult.Fail(AuthorizationResultReason.Empty);
					case EmptyBehavior.Alow:
						return AuthorizationResult.OK();
					default:
						throw new NotSupportedException();
				}
			}

			bool denyFound = false;
			bool allowFound = false;

			foreach (var i in permissions)
			{
				foreach (var j in Providers)
				{
					var r = j.Authorize(context, i, e, state);

					switch (r)
					{
						case AuthorizationProviderResult.Success:
							allowFound = true;
							break;
						case AuthorizationProviderResult.Fail:
							denyFound = true;
							break;
					}

					if (r == AuthorizationProviderResult.Success && e.Schema.Level == AuthorizationLevel.Optimistic)
						return AuthorizationResult.OK();
					else if (r == AuthorizationProviderResult.Fail && e.Schema.Level == AuthorizationLevel.Pessimistic)
						return AuthorizationResult.Fail(AuthorizationResultReason.Other);
				}
			}

			switch (e.Schema.Level)
			{
				case AuthorizationLevel.Pessimistic:
					if (allowFound)
						return AuthorizationResult.OK();
					else
						return AuthorizationResult.Fail(AuthorizationResultReason.NoAllowFound);
				case AuthorizationLevel.Optimistic:
					if (denyFound)
						return AuthorizationResult.Fail(AuthorizationResultReason.DenyFound);
					else
						return AuthorizationResult.OK();
				default:
					throw new NotSupportedException();
			}
		}

		public bool Demand(Guid user, Guid role)
		{
			return Membership.Select(user, role) != null;
		}

		public void NotifyMembershipAdded(object sender, MembershipEventArgs e)
		{
			Membership.Add(e.User, e.Role);
		}

		public void NotifyMembershipRemoved(object sender, MembershipEventArgs e)
		{
			Membership.Remove(e.User, e.Role);
		}

		public void NotifyPermissionAdded(object sender, PermissionEventArgs e)
		{
			LoadPermission(e);
		}

		public void NotifyPermissionRemoved(object sender, PermissionEventArgs e)
		{
			Remove(f => f.Evidence == e.Evidence
				&& f.Schema.Equals(e.Schema, StringComparison.OrdinalIgnoreCase)
				&& f.Claim.Equals(e.Claim, StringComparison.OrdinalIgnoreCase)
				&& f.PrimaryKey.Equals(e.PrimaryKey, StringComparison.OrdinalIgnoreCase));
		}

		public void NotifyPermissionChanged(object sender, PermissionEventArgs e)
		{
			Remove(f => f.Evidence == e.Evidence
				&& f.Schema.Equals(e.Schema, StringComparison.OrdinalIgnoreCase)
				&& f.Claim.Equals(e.Claim, StringComparison.OrdinalIgnoreCase)
				&& f.PrimaryKey.Equals(e.PrimaryKey, StringComparison.OrdinalIgnoreCase));

			LoadPermission(e);
		}

		public void RegisterProvider(IAuthorizationProvider provider)
		{
			Providers.Add(provider);
		}

		public List<IAuthorizationProvider> QueryProviders()
		{
			return Providers;
		}

		public void RegisterDescriptor(IPermissionDescriptor descriptor)
		{
			Descriptors.Add(descriptor);
		}

		public List<IPermissionDescriptor> QueryDescriptors()
		{
			return Descriptors;
		}

		public PermissionValue GetPermissionValue(Guid evidence, string schema, string claim)
		{
			var r = Get(f => f.Evidence == evidence
				  && f.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase)
				  && f.Claim.Equals(claim, StringComparison.OrdinalIgnoreCase));

			if (r == null)
				return PermissionValue.NotSet;

			return r.Value;
		}

		private List<IAuthorizationProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new List<IAuthorizationProvider>();

				return _providers;
			}
		}

		private void LoadPermission(PermissionEventArgs e)
		{
			var u = Connection.CreateUrl("Security", "SelectPermission")
				.AddParameter("evidence", e.Evidence)
				.AddParameter("schema", e.Schema)
				.AddParameter("claim", e.Claim)
				.AddParameter("primaryKey", e.PrimaryKey);

			var d = Connection.Get<Permission>(u);

			if (d != null)
				Set(GenerateRandomKey(), d, TimeSpan.Zero);
		}

		public List<IMembership> QueryMembership(Guid user)
		{
			return Membership.Query(user);
		}

		public bool IsInRole(Guid user, string role)
		{
			var r = Connection.GetService<IRoleService>().Select(role);

			if (r == null)
				return false;

			if (r.Behavior == RoleBehavior.Implicit)
				return RoleAuthorizationProvider.IsInImplicitRole(Connection, user, r);

			return Membership.Select(user, r.Token) != null;
		}

		public void RegisterAuthenticationProvider(IAuthenticationProvider provider)
		{
			AuthenticationProviders.Add(provider);
		}

		private MembershipCache Membership { get; }
		private List<IPermissionDescriptor> Descriptors => _descriptors.Value;
		private List<IAuthenticationProvider> AuthenticationProviders { get { return _authProviders.Value; } }
		private IAuthenticationProvider DefaultAuthenticationProvider
		{
			get
			{
				if (_defaultAuthenticationProvider == null)
					_defaultAuthenticationProvider = new DefaultAuthenticationProvider(Connection);

				return _defaultAuthenticationProvider;
			}
		}
	}
}
