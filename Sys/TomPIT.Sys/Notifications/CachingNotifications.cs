﻿using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Messaging;
using TomPIT.Reflection;
using TomPIT.Security;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Notifications
{
	public static class CachingNotifications
	{
		internal static IHubContext<CacheHub> Cache { get; set; }

		private static void Notify<T>(string method, T e)
		{
			var args = new MessageEventArgs<T>(Guid.NewGuid(), e);

			var state = new CacheState
			{
				Method = method,
				Type = args.GetType().TypeName(),
				Content = JsonConvert.SerializeObject(args)
			};

			DataModel.Messages.Insert("cache", args.Message, JsonConvert.SerializeObject(state), DateTime.UtcNow.AddMinutes(1), TimeSpan.FromSeconds(3), SysExtensions.RequestInstanceId);

			if (Cache != null)
			{
				var sender = SysExtensions.RequestConnectionId("cache");

				if (string.IsNullOrWhiteSpace(sender))
					Cache.Clients.All.SendAsync(method, args).Wait();
				else
					Cache.Clients.AllExcept(sender).SendAsync(method, args).Wait();
			}
		}

		public static void AuthenticationTokenChanged(Guid token) { Notify(nameof(AuthenticationTokenChanged), new AuthenticationTokenEventArgs(token)); }
		public static void AuthenticationTokenRemoved(Guid token) { Notify(nameof(AuthenticationTokenRemoved), new AuthenticationTokenEventArgs(token)); }
		public static void FolderChanged(Guid microService, Guid folder) { Notify(nameof(FolderChanged), new FolderEventArgs(microService, folder)); }
		public static void FolderRemoved(Guid microService, Guid folder) { Notify(nameof(FolderRemoved), new FolderEventArgs(microService, folder)); }
		public static void ResourceGroupChanged(Guid resourceGroup) { Notify(nameof(ResourceGroupChanged), new ResourceGroupEventArgs(resourceGroup)); }
		public static void ResourceGroupRemoved(Guid resourceGroup) { Notify(nameof(ResourceGroupRemoved), new ResourceGroupEventArgs(resourceGroup)); }
		public static void SettingChanged(string name, string nameSpace, string type, string primaryKey) { Notify(nameof(SettingChanged), new SettingEventArgs(name, nameSpace, type, primaryKey)); }
		public static void SettingRemoved(string name, string nameSpace, string type, string primaryKey) { Notify(nameof(SettingRemoved), new SettingEventArgs(name, nameSpace, type, primaryKey)); }
		public static void MicroServiceChanged(Guid microService) { Notify(nameof(MicroServiceChanged), new MicroServiceEventArgs(microService)); }
		public static void MicroServiceInstalled(Guid microService, bool success) { Notify(nameof(MicroServiceInstalled), new MicroServiceInstallEventArgs(microService, success)); }
		public static void MicroServiceRemoved(Guid microService) { Notify(nameof(MicroServiceRemoved), new MicroServiceEventArgs(microService)); }
		public static void LanguageChanged(Guid language) { Notify(nameof(LanguageChanged), new LanguageEventArgs(language)); }
		public static void LanguageRemoved(Guid language) { Notify(nameof(LanguageRemoved), new LanguageEventArgs(language)); }
		public static void MicroServiceStringChanged(Guid microService, Guid language, Guid element, string property) { Notify(nameof(MicroServiceStringChanged), new MicroServiceStringEventArgs(microService, language, element, property)); }
		public static void MicroServiceStringRemoved(Guid microService, Guid element, string property) { Notify(nameof(MicroServiceStringRemoved), new MicroServiceStringEventArgs(microService, Guid.Empty, element, property)); }
		/*
		 * Blobs
		 */
		public static void BlobCommitted(Guid microService, Guid blob, int type, string primaryKey) { Notify(nameof(BlobCommitted), new BlobEventArgs(microService, blob, type, primaryKey)); }
		public static void BlobRemoved(Guid microService, Guid blob, int type, string primaryKey) { Notify(nameof(BlobRemoved), new BlobEventArgs(microService, blob, type, primaryKey)); }
		public static void BlobChanged(Guid microService, Guid blob, int type, string primaryKey) { Notify(nameof(BlobChanged), new BlobEventArgs(microService, blob, type, primaryKey)); }
		public static void BlobAdded(Guid microService, Guid blob, int type, string primaryKey) { Notify(nameof(BlobChanged), new BlobEventArgs(microService, blob, type, primaryKey)); }
		/*
		 * Data
		 */
		public static void UserDataChanged(Guid user) { Notify(nameof(UserDataChanged), new UserEventArgs(user)); }
		/*
		 * Security
		 */
		public static void UserChanged(Guid user) { Notify(nameof(UserChanged), new UserEventArgs(user)); }
		public static void AlienChanged(Guid alien) { Notify(nameof(AlienChanged), new AlienEventArgs(alien)); }
		public static void RoleChanged(Guid role) { Notify(nameof(RoleChanged), new RoleEventArgs(role)); }
		public static void ClientChanged(string client) { Notify(nameof(ClientChanged), new ClientEventArgs(client)); }
		public static void MembershipAdded(Guid user, Guid role) { Notify(nameof(MembershipAdded), new MembershipEventArgs(user, role)); }
		public static void MembershipRemoved(Guid user, Guid role) { Notify(nameof(MembershipRemoved), new MembershipEventArgs(user, role)); }
		public static void PermissionAdded(Guid resourceGroup, string evidence, string schema, string claim, string primaryKey, string descriptor) { Notify(nameof(PermissionAdded), new PermissionEventArgs(resourceGroup, evidence, schema, claim, primaryKey, descriptor)); }
		public static void PermissionChanged(Guid resourceGroup, string evidence, string schema, string claim, string primaryKey, string descriptor) { Notify(nameof(PermissionChanged), new PermissionEventArgs(resourceGroup, evidence, schema, claim, primaryKey, descriptor)); }
		public static void PermissionRemoved(Guid resourceGroup, string evidence, string schema, string claim, string primaryKey, string descriptor) { Notify(nameof(PermissionRemoved), new PermissionEventArgs(resourceGroup, evidence, schema, claim, primaryKey, descriptor)); }
		public static void ComponentChanged(Guid microService, Guid folder, Guid component, string nameSpace, string category, string name) { Notify(nameof(ComponentChanged), new ComponentEventArgs(microService, folder, component, nameSpace, category, name)); }
		public static void ComponentRemoved(Guid microService, Guid folder, Guid component, string nameSpace, string category, string name) { Notify(nameof(ComponentRemoved), new ComponentEventArgs(microService, folder, component, nameSpace, category, name)); }
		public static void ComponentAdded(Guid microService, Guid folder, Guid component, string nameSpace, string category, string name) { Notify(nameof(ComponentAdded), new ComponentEventArgs(microService, folder, component, nameSpace, category, name)); }
		public static void XmlKeyChanged(string id) { Notify(nameof(XmlKeyChanged), new XmlKeyEventArgs(id)); }
		/*
		 * Configuration
		 */
		public static void ConfigurationChanged(Guid microService, Guid configuration, string category) { Notify(nameof(ConfigurationChanged), new ConfigurationEventArgs(microService, configuration, category)); }
		public static void ConfigurationRemoved(Guid microService, Guid configuration, string category) { Notify(nameof(ConfigurationRemoved), new ConfigurationEventArgs(microService, configuration, category)); }
		public static void ConfigurationAdded(Guid microService, Guid configuration, string category) { Notify(nameof(ConfigurationAdded), new ConfigurationEventArgs(microService, configuration, category)); }

		public static void SourceTextChanged(Guid microService, Guid configuration, Guid token, int type) { Notify(nameof(SourceTextChanged), new SourceTextChangedEventArgs(microService, configuration, token, type)); }
	}
}
