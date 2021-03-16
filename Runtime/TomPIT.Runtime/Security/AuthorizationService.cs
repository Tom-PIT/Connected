using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Caching;
using TomPIT.Collections;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Navigation;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;
using TomPIT.Security.Authentication;
using TomPIT.Security.AuthorizationProviders;

namespace TomPIT.Security
{
	internal class AuthorizationService : SynchronizedClientRepository<IPermission, string>,
		IAuthorizationService, IAuthorizationNotification, IMembershipProvider, IAuthenticationTokenNotification, IPermissionService, IAuthenticationTokenProvider
	{
		private List<IAuthorizationProvider> _providers = null;
		private Lazy<List<IPermissionDescriptor>> _descriptors = new Lazy<List<IPermissionDescriptor>>();
		private Lazy<List<IAuthenticationProvider>> _authProviders = new Lazy<List<IAuthenticationProvider>>();
		private IAuthenticationProvider _defaultAuthenticationProvider = null;

		public AuthorizationService(ITenant tenant) : base(tenant, "permissions")
		{
			Membership = new MembershipCache(tenant);
			AuthenticationTokens = new AuthenticationTokensCache(tenant);

			Providers.Add(new RoleAuthorizationProvider());
			Providers.Add(new UserAuthorizationProvider());
			Providers.Add(new PolicyAuthorizationProvider());
		}

		protected override void OnInitializing()
		{
			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
			{
				var ds = Tenant.Get<ImmutableList<Permission>>(CreateUrl("QueryPermissions")).ToList<IPermission>();

				foreach (var i in ds)
					Set(GenerateRandomKey(), i, TimeSpan.Zero);
			}
			else
			{
				var ds = Tenant.Post<ImmutableList<Permission>>(CreateUrl("QueryPermissionsForResourceGroup"), new
				{
					Data = Shell.GetConfiguration<IClientSys>().ResourceGroups
				}).ToList<IPermission>();

				foreach (var i in ds)
					Set(GenerateRandomKey(), i, TimeSpan.Zero);
			}
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

		public IClientAuthenticationResult AuthenticateByPin(string user, string pin)
		{
			foreach (var i in AuthenticationProviders)
			{
				var r = i.AuthenticateByPin(user, pin);

				if (r != null)
					return r;
			}

			return DefaultAuthenticationProvider.AuthenticateByPin(user, pin);
		}

		public IClientAuthenticationResult Authenticate(string authenticationToken)
		{
			foreach (var i in AuthenticationProviders)
			{
				var r = i.Authenticate(authenticationToken);

				if (r != null)
					return r;
			}

			return DefaultAuthenticationProvider.Authenticate(authenticationToken);
		}

		public IClientAuthenticationResult Authenticate(Guid authenticationToken)
		{
			foreach (var i in AuthenticationProviders)
			{
				var r = i.Authenticate(authenticationToken);

				if (r != null)
					return r;
			}

			return DefaultAuthenticationProvider.Authenticate(authenticationToken);
		}

		public IAuthorizationResult Authorize(IMiddlewareContext context, AuthorizationArgs e)
		{
			if (context is IElevationContext ec && ec.State == ElevationContextState.Granted)
				return AuthorizationResult.OK(0);

			if (string.IsNullOrWhiteSpace(e.Claim))
				return AuthorizationResult.Fail(AuthorizationResultReason.NoClaim, 0);

			if (string.IsNullOrWhiteSpace(e.PrimaryKey))
				return AuthorizationResult.Fail(AuthorizationResultReason.NoPrimaryKey, 0);

			if (string.IsNullOrWhiteSpace(e.PermissionDescriptor))
				return AuthorizationResult.Fail(AuthorizationResultReason.NoPermissionDescriptor, 0);

			var permissions = Where(f => string.Compare(f.Claim, e.Claim, true) == 0
				&& string.Compare(f.PrimaryKey, e.PrimaryKey, true) == 0
				&& string.Compare(f.Descriptor, e.PermissionDescriptor, true) == 0);

			var state = new Dictionary<string, object>();

			foreach (var i in Providers)
			{
				if (i.PreAuthorize(context, e, state) == AuthorizationProviderResult.Success)
					return AuthorizationResult.OK(permissions.Count);
			}

			if (permissions.Count == 0)
			{
				return e.Schema.Empty switch
				{
					EmptyBehavior.Deny => AuthorizationResult.Fail(AuthorizationResultReason.Empty, permissions.Count),
					EmptyBehavior.Alow => AuthorizationResult.OK(permissions.Count),
					_ => throw new NotSupportedException(),
				};
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
						return AuthorizationResult.OK(permissions.Count);
					else if (r == AuthorizationProviderResult.Fail && e.Schema.Level == AuthorizationLevel.Pessimistic)
						return AuthorizationResult.Fail(AuthorizationResultReason.Other, permissions.Count);
				}
			}

			switch (e.Schema.Level)
			{
				case AuthorizationLevel.Pessimistic:
					if (allowFound)
						return AuthorizationResult.OK(permissions.Count);
					else
						return AuthorizationResult.Fail(AuthorizationResultReason.NoAllowFound, permissions.Count);
				case AuthorizationLevel.Optimistic:
					if (denyFound)
						return AuthorizationResult.Fail(AuthorizationResultReason.DenyFound, permissions.Count);
					else
						return AuthorizationResult.OK(permissions.Count);
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
			if (!Instance.ResourceGroupExists(e.ResourceGroup))
				return;

			LoadPermission(e);
		}

		public void NotifyAuthenticationTokenChanged(object sender, AuthenticationTokenEventArgs e)
		{
			AuthenticationTokens.NotifyChanged(e.Token);
		}

		public void NotifyAuthenticationTokenRemoved(object sender, AuthenticationTokenEventArgs e)
		{
			AuthenticationTokens.NotifyRemoved(e.Token);
		}

		public void NotifyPermissionRemoved(object sender, PermissionEventArgs e)
		{
			if (!Instance.ResourceGroupExists(e.ResourceGroup))
				return;

			Remove(f => string.Compare(f.Evidence, e.Evidence, true) == 0
				&& f.Schema.Equals(e.Schema, StringComparison.OrdinalIgnoreCase)
				&& f.Claim.Equals(e.Claim, StringComparison.OrdinalIgnoreCase)
				&& f.PrimaryKey.Equals(e.PrimaryKey, StringComparison.OrdinalIgnoreCase)
				&& string.Compare(f.Descriptor, e.Descriptor, true) == 0);
		}

		public void NotifyPermissionChanged(object sender, PermissionEventArgs e)
		{
			if (!Instance.ResourceGroupExists(e.ResourceGroup))
				return;

			Remove(f => string.Compare(f.Evidence, e.Evidence, true) == 0
				&& f.Schema.Equals(e.Schema, StringComparison.OrdinalIgnoreCase)
				&& f.Claim.Equals(e.Claim, StringComparison.OrdinalIgnoreCase)
				&& f.PrimaryKey.Equals(e.PrimaryKey, StringComparison.OrdinalIgnoreCase)
				&& string.Compare(f.Descriptor, e.Descriptor, true) == 0);

			LoadPermission(e);
		}

		public void RegisterProvider(IAuthorizationProvider provider)
		{
			Providers.Add(provider);
		}

		public ImmutableList<IAuthorizationProvider> QueryProviders()
		{
			return Providers.ToImmutableList();
		}

		public void RegisterDescriptor(IPermissionDescriptor descriptor)
		{
			Descriptors.Add(descriptor);
		}

		public ImmutableList<IPermissionDescriptor> QueryDescriptors()
		{
			return Descriptors.ToImmutableList();
		}

		public PermissionValue GetPermissionValue(string evidence, string schema, string claim, string descriptor)
		{
			var r = Get(f => string.Compare(f.Evidence, evidence, true) == 0
					&& f.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase)
					&& f.Claim.Equals(claim, StringComparison.OrdinalIgnoreCase)
					&& string.Compare(f.Descriptor, descriptor, true) == 0);

			if (r == null)
				return PermissionValue.NotSet;

			return r.Value;
		}

		public PermissionValue GetPermissionValue(string evidence, string schema, string claim, string descriptor, string primaryKey)
		{
			var r = Get(f => string.Compare(f.Evidence, evidence, true) == 0
				  && f.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase)
				  && f.Claim.Equals(claim, StringComparison.OrdinalIgnoreCase)
				  && string.Compare(f.Descriptor, descriptor, true) == 0
				  && string.Compare(f.PrimaryKey, primaryKey, true) == 0);

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
			var u = Tenant.CreateUrl("Security", "SelectPermission")
				.AddParameter("evidence", e.Evidence)
				.AddParameter("schema", e.Schema)
				.AddParameter("claim", e.Claim)
				.AddParameter("primaryKey", e.PrimaryKey)
				.AddParameter("descriptor", e.Descriptor);

			var d = Tenant.Get<Permission>(u);

			if (d != null)
				Set(GenerateRandomKey(), d, TimeSpan.Zero);
		}

		public ImmutableList<IMembership> QueryMembership(Guid user)
		{
			return Membership.Query(user);
		}

		public bool IsInRole(Guid user, string role)
		{
			var r = Tenant.GetService<IRoleService>().Select(role);

			if (r == null)
				return false;

			if (r.Behavior == RoleBehavior.Implicit)
				return RoleAuthorizationProvider.IsInImplicitRole(Tenant, user, r);

			return Membership.Select(user, r.Token) != null;
		}

		public void RegisterAuthenticationProvider(IAuthenticationProvider provider)
		{
			AuthenticationProviders.Add(provider);
		}

		public void Authorize(ISiteMapContainer container)
		{
			if (container == null)
				return;

			using var context = new MiddlewareContext(Tenant.Url);

			Authorize(context, container.Routes, context.Services.Identity.IsAuthenticated
				? context.Services.Identity.User.Token
				: Guid.Empty);
		}

		private void Authorize(IMiddlewareContext context, ConnectedList<ISiteMapRoute, ISiteMapContainer> routes, Guid user)
		{
			for (var i = routes.Count - 1; i >= 0; i--)
			{
				var route = routes[i];

				if (route is ISiteMapAuthorizationElement ae)
				{
					if (!ae.Authorize(context, user))
						routes.RemoveAt(i);
					else
						Authorize(context, route.Routes, user);
				}
			}
		}

		private void Authorize(IMiddlewareContext context, ConnectedList<ISiteMapRoute, ISiteMapRoute> routes, Guid user)
		{
			for (var i = routes.Count - 1; i >= 0; i--)
			{
				var route = routes[i];

				if (route is ISiteMapAuthorizationElement ae)
				{
					if (!ae.Authorize(context, user))
						routes.RemoveAt(i);
					else
						Authorize(context, route.Routes, user);
				}
			}
		}

		PermissionValue IPermissionService.Toggle(string claim, string schema, string evidence, string primaryKey, string permissionDescriptor)
		{
			var value = Tenant.Post<PermissionValue>(CreateManagementUrl("SetPermission"), new
			{
				claim,
				schema,
				Descriptor = permissionDescriptor,
				primaryKey,
				evidence
			});

			NotifyPermissionChanged(this, new PermissionEventArgs(Guid.Empty, evidence, schema, claim, primaryKey, permissionDescriptor));

			return value;
		}

		void IPermissionService.Reset(string claim, string schema, string primaryKey, string descriptor)
		{
			Tenant.Post(CreateManagementUrl("Reset"), new
			{
				claim,
				schema,
				primaryKey,
				descriptor
			});

			var permissions = Where(f => string.Compare(f.Claim, claim, true) == 0
				&& string.Compare(f.Schema, schema, true) == 0
				&& string.Compare(f.PrimaryKey, primaryKey, true) == 0
				&& string.Compare(f.Descriptor, descriptor, true) == 0);

			foreach (var permission in permissions)
				NotifyPermissionRemoved(this, new PermissionEventArgs(Guid.Empty, permission.Evidence, permission.Schema, permission.Claim, permission.PrimaryKey, permission.Descriptor));
		}

		void IPermissionService.Reset(string primaryKey)
		{
			Tenant.Post(CreateManagementUrl("Reset"), new
			{
				primaryKey
			});

			var permissions = Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);

			foreach (var permission in permissions)
				NotifyPermissionRemoved(this, new PermissionEventArgs(Guid.Empty, permission.Evidence, permission.Schema, permission.Claim, permission.PrimaryKey, permission.Descriptor));
		}

		ImmutableList<IPermission> IPermissionService.Query(string permissionDescriptor, string primaryKey)
		{
			return Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0 && string.Compare(f.Descriptor, permissionDescriptor, true) == 0);
		}

		ImmutableList<IPermission> IPermissionService.Query(string permissionDescriptor, Guid user)
		{
			var membership = QueryMembership(user);
			var result = new List<IPermission>();

			foreach (var m in membership)
				result.AddRange(Where(f => string.Compare(f.Evidence, m.Role.ToString(), true) == 0 && string.Compare(f.Descriptor, permissionDescriptor, true) == 0));

			result.AddRange(Where(f => string.Compare(f.Evidence, user.ToString(), true) == 0 && string.Compare(f.Descriptor, permissionDescriptor, true) == 0));

			return result.ToImmutableList();
		}

		private MembershipCache Membership { get; }
		internal AuthenticationTokensCache AuthenticationTokens { get; }
		private List<IPermissionDescriptor> Descriptors => _descriptors.Value;
		private List<IAuthenticationProvider> AuthenticationProviders => _authProviders.Value;
		private IAuthenticationProvider DefaultAuthenticationProvider
		{
			get
			{
				if (_defaultAuthenticationProvider == null)
					_defaultAuthenticationProvider = new DefaultAuthenticationProvider(Tenant);

				return _defaultAuthenticationProvider;
			}
		}

		public ImmutableList<IMembership> QueryMembershipForRole(Guid role)
		{
			return Membership.QueryForRole(role);
		}

		public void AuthorizePolicies(IMiddlewareContext context, object instance)
		{
			AuthorizePolicies(context, instance, null);
		}
		public void AuthorizePolicies(IMiddlewareContext context, object instance, string method)
		{
			var attributes = instance.GetType().GetCustomAttributes(true);

			var targets = new List<AuthorizationPolicyAttribute>();

			foreach (var attribute in attributes)
			{
				if (attribute is not AuthorizationPolicyAttribute policy || policy.MiddlewareStage == AuthorizationMiddlewareStage.Result)
					continue;

				if (string.Compare(method, policy.Method, true) == 0)
					targets.Add(policy);
			}

			Exception firstFail = null;
			bool onePassed = false;

			foreach (var attribute in targets.OrderByDescending(f => f.Priority))
			{
				try
				{
					if (attribute.Behavior == AuthorizationPolicyBehavior.Optional && onePassed)
						continue;

					attribute.Authorize(context, instance);

					onePassed = true;
				}
				catch (Exception ex)
				{
					if (attribute.Behavior == AuthorizationPolicyBehavior.Mandatory)
						throw;

					firstFail = ex;
				}
			}

			if (!onePassed && firstFail != null)
				throw firstFail;
		}

		public string RequestToken(InstanceType type)
		{
			return DefaultAuthenticationProvider.RequestToken(type);
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl("Security", action);
		}

		private ServerUrl CreateManagementUrl(string action)
		{
			return Tenant.CreateUrl("SecurityManagement", action);
		}
	}
}
