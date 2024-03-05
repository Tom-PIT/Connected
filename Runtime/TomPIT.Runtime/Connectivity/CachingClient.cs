using Microsoft.AspNetCore.SignalR.Client;

using System;
using System.Threading.Tasks;

using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Data;
using TomPIT.Globalization;
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

		protected override string HubName => "hubs/caching";

		protected override void Initialize()
		{
			Users();
			Roles();
			Blobs();
			Configuration();
			Security();
			MicroServices();
			AuthenticationTokens();
			Data();
			Settings();
			Globalization();
		}

		private void Globalization()
		{
			Hub.On<MessageEventArgs<LanguageEventArgs>>("LanguageChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<ILanguageService>() is ILanguageNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<LanguageEventArgs>>("LanguageRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<ILanguageService>() is ILanguageNotification n)
						n.NotifyRemoved(Tenant, e.Args);
				})
			);
		}

		private void Settings()
		{
			Hub.On<MessageEventArgs<SettingEventArgs>>("SettingChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<ISettingService>() is ISettingNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<SettingEventArgs>>("SettingRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<ISettingService>() is ISettingNotification n)
						n.NotifyRemoved(Tenant, e.Args);
				})
			);
		}

		private void Data()
		{
			Hub.On<MessageEventArgs<UserEventArgs>>("UserDataChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IUserDataService>() is IUserNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);
		}

		private void AuthenticationTokens()
		{
			Hub.On<MessageEventArgs<AuthenticationTokenEventArgs>>("AuthenticationTokenChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
						n.NotifyAuthenticationTokenChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<AuthenticationTokenEventArgs>>("AuthenticationTokenRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
						n.NotifyAuthenticationTokenRemoved(Tenant, e.Args);
				})
			);
		}

		private void MicroServices()
		{
			Hub.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
						n.NotifyRemoved(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<MicroServiceInstallEventArgs>>("MicroServiceInstalled", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
						n.NotifyMicroServiceInstalled(Tenant, e.Args);
				})
			);
		}

		private void Security()
		{
			Hub.On<MessageEventArgs<MembershipEventArgs>>("MembershipAdded", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
						n.NotifyMembershipAdded(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<MembershipEventArgs>>("MembershipRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
						n.NotifyMembershipRemoved(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionAdded", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
						n.NotifyPermissionAdded(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
						n.NotifyPermissionRemoved(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<PermissionEventArgs>>("PermissionChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
						n.NotifyPermissionChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<XmlKeyEventArgs>>("XmlKeyChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IXmlKeyService>() is IXmlKeyNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);
		}

		private void Configuration()
		{
			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationAdded", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyAdded(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyRemoved(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<ComponentEventArgs>>("ComponentAdded", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyAdded(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<ComponentEventArgs>>("ComponentChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<ComponentEventArgs>>("ComponentRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyRemoved(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<SourceTextChangedEventArgs>>("SourceTextChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<ICompilerService>() is ICompilerNotification n)
						n.NotifyChanged(Tenant, e.Args);

					if (Tenant.GetService<IComponentService>() is IComponentNotification cn)
						cn.NotifySourceTextChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<FolderEventArgs>>("FolderChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyFolderChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<FolderEventArgs>>("FolderRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IComponentService>() is IComponentNotification n)
						n.NotifyFolderRemoved(Tenant, e.Args);
				})
			);
		}

		private void Users()
		{
			Hub.On<MessageEventArgs<UserEventArgs>>("UserChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IUserService>() is IUserNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<AlienEventArgs>>("AlienChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IAlienService>() is IAlienNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);
		}

		private void Roles()
		{
			Hub.On<MessageEventArgs<RoleEventArgs>>("RoleChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IRoleService>() is IRoleNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);
		}

		private void Blobs()
		{
			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobChanged", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IStorageService>() is IStorageNotification n)
						n.NotifyChanged(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobAdded", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IStorageService>() is IStorageNotification n)
						n.NotifyAdded(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobRemoved", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IStorageService>() is IStorageNotification n)
						n.NotifyRemoved(Tenant, e.Args);
				})
			);

			Hub.On<MessageEventArgs<BlobEventArgs>>("BlobCommitted", (e) =>
				ConfirmAndHandle(e, (e) =>
				{
					if (Tenant.GetService<IStorageService>() is IStorageNotification n)
						n.NotifyCommitted(Tenant, e.Args);
				})
			);
		}
	}
}
