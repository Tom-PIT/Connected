using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Environment;
using TomPIT.Notifications;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Connectivity
{
	internal class CachingEvents
	{
		public CachingEvents(ISysConnection connection, HubConnection hub)
		{
			Hub = hub;
			Connection = connection;
		}

		private HubConnection Hub { get; }
		private ISysConnection Connection { get; }

		public void Hook()
		{
			HookUsers();
			HookRoles();
			HookFeatures();
			HookBlobs();
			HookConfiguration();
			HookSecurity();
			HookMicroServices();
			HookInstances();
			HookEnvironmentUnits();
			HookAuthenticationTokens();
		}

		private void HookAuthenticationTokens()
		{
			Hub.On<MessageEventArgs<AuthenticationTokenEventArgs>>("AuthenticationTokenChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
					n.NotifyAuthenticationTokenChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<AuthenticationTokenEventArgs>>("AuthenticationTokenRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
					n.NotifyAuthenticationTokenRemoved(Connection, e.Args);
			});
		}

		private void HookEnvironmentUnits()
		{
			Hub.On<MessageEventArgs<EnvironmentUnitEventArgs>>("EnvironmentUnitChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
					n.NotifyChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<EnvironmentUnitEventArgs>>("EnvironmentUnitRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
					n.NotifyRemoved(Connection, e.Args);
			});
		}

		private void HookInstances()
		{
			Hub.On<MessageEventArgs<InstanceEndpointEventArgs>>("InstanceEndpointChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
					n.NotifyChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<InstanceEndpointEventArgs>>("InstanceEndpointRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
					n.NotifyRemoved(Connection, e.Args);
			});
		}

		private void HookMicroServices()
		{
			Hub.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyRemoved(Connection, e.Args);
			});
		}

		private void HookSecurity()
		{
			Hub.On<MessageEventArgs<MembershipEventArgs>>("MembershipAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyMembershipAdded(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<MembershipEventArgs>>("MembershipRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyMembershipRemoved(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionAdded(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionRemoved(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionChanged(Connection, e.Args);
			});
		}

		private void HookConfiguration()
		{
			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyAdded(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyRemoved(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<ScriptChangedEventArgs>>("ScriptChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<ICompilerService>() is ICompilerNotification n)
					n.NotifyChanged(Connection, e.Args);
			});
		}

		private void HookUsers()
		{
			Hub.On<MessageEventArgs<UserEventArgs>>("UserChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IUserService>() is IUserNotification n)
					n.NotifyChanged(Connection, e.Args);
			});
		}

		private void HookRoles()
		{
			Hub.On<MessageEventArgs<RoleEventArgs>>("RoleChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IRoleService>() is IRoleNotification n)
					n.NotifyChanged(Connection, e.Args);
			});
		}

		private void HookFeatures()
		{
			Hub.On<MessageEventArgs<FeatureEventArgs>>("FeatureChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IFeatureService>() is IFeatureNotification n)
					n.NotifyChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<FeatureEventArgs>>("FeatureRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IFeatureService>() is IFeatureNotification n)
					n.NotifyChanged(Connection, e.Args);
			});
		}

		private void HookBlobs()
		{
			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyChanged(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyAdded(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyRemoved(Connection, e.Args);
			});

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobCommitted", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Connection.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyCommitted(Connection, e.Args);
			});
		}
	}
}
