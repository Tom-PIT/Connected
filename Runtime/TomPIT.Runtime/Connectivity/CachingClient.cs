using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Data;
using TomPIT.Environment;
using TomPIT.Messaging;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Connectivity
{
	internal class CachingClient : HubClient
	{
		public CachingClient(ITenant tenant, string authenticationToken) : base(tenant, authenticationToken)
		{
		}

		protected override string HubName => "caching";

		protected override void Initialize()
		{
			Users();
			Roles();
			Blobs();
			Configuration();
			Security();
			MicroServices();
			Instances();
			AuthenticationTokens();
			Data();
			Settings();
			Globalization();
		}

		private void Globalization()
		{
			Hub.On<MessageEventArgs<SettingEventArgs>>("SettingChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<ISettingService>() is ISettingNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<SettingEventArgs>>("SettingRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<ISettingService>() is ISettingNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});
		}

		private void Settings()
		{
			Hub.On<MessageEventArgs<SettingEventArgs>>("SettingChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<ISettingService>() is ISettingNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<SettingEventArgs>>("SettingRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<ISettingService>() is ISettingNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});
		}

		private void Data()
		{
			Hub.On<MessageEventArgs<UserEventArgs>>("UserDataChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IUserDataService>() is IUserNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});
		}

		private void AuthenticationTokens()
		{
			Hub.On<MessageEventArgs<AuthenticationTokenEventArgs>>("AuthenticationTokenChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
					n.NotifyAuthenticationTokenChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<AuthenticationTokenEventArgs>>("AuthenticationTokenRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
					n.NotifyAuthenticationTokenRemoved(Tenant, e.Args);
			});
		}
		private void Instances()
		{
			Hub.On<MessageEventArgs<InstanceEndpointEventArgs>>("InstanceEndpointChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<InstanceEndpointEventArgs>>("InstanceEndpointRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});
		}

		private void MicroServices()
		{
			Hub.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<MicroServiceInstallEventArgs>>("MicroServiceInstalled", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyMicroServiceInstalled(Tenant, e.Args);
			});
		}

		private void Security()
		{
			Hub.On<MessageEventArgs<MembershipEventArgs>>("MembershipAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyMembershipAdded(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<MembershipEventArgs>>("MembershipRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyMembershipRemoved(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionAdded(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionRemoved(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionChanged(Tenant, e.Args);
			});
		}

		private void Configuration()
		{
			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyAdded(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<ComponentEventArgs>>("ComponentAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyAdded(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<ComponentEventArgs>>("ComponentChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<ComponentEventArgs>>("ComponentRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<ScriptChangedEventArgs>>("ScriptChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<ICompilerService>() is ICompilerNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<FolderEventArgs>>("FolderChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyFolderChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<FolderEventArgs>>("FolderRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyFolderRemoved(Tenant, e.Args);
			});
		}

		private void Users()
		{
			Hub.On<MessageEventArgs<UserEventArgs>>("UserChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IUserService>() is IUserNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<AlienEventArgs>>("AlienChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IAlienService>() is IAlienNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});
		}

		private void Roles()
		{
			Hub.On<MessageEventArgs<RoleEventArgs>>("RoleChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IRoleService>() is IRoleNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});
		}

		private void Blobs()
		{
			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyChanged(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyAdded(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyRemoved(Tenant, e.Args);
			});

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobCommitted", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyCommitted(Tenant, e.Args);
			});
		}
	}
}
