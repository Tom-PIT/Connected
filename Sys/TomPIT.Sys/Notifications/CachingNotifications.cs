﻿using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Notifications;
using TomPIT.Security;
using TomPIT.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	internal static class CachingNotifications
	{
		internal static IHubContext<CacheHub> Cache { get; set; }

		private static async void Notify<T>(string method, T e)
		{
			var args = new MessageEventArgs<T>(Guid.NewGuid(), e);

			var state = new CacheState
			{
				Method = method,
				Type = args.GetType().TypeName(),
				Content = JsonConvert.SerializeObject(args)
			};

			DataModel.Messages.Insert("cache", args.Message, JsonConvert.SerializeObject(state), DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5), SysExtensions.RequestInstanceId);

			if (Cache != null)
			{
				var sender = SysExtensions.RequestConnectionId("cache");

				if (string.IsNullOrWhiteSpace(sender))
					await Cache.Clients.All.SendAsync(method, args);
				else
					await Cache.Clients.AllExcept(sender).SendAsync(method, args);
			}
		}

		public static void AuthenticationTokenChanged(Guid token) { Notify(nameof(AuthenticationTokenChanged), new AuthenticationTokenEventArgs(token)); }
		public static void AuthenticationTokenRemoved(Guid token) { Notify(nameof(AuthenticationTokenRemoved), new AuthenticationTokenEventArgs(token)); }
		public static void FolderChanged(Guid microService, Guid folder) { Notify(nameof(FolderChanged), new FolderEventArgs(microService, folder)); }
		public static void FolderRemoved(Guid microService, Guid folder) { Notify(nameof(FolderRemoved), new FolderEventArgs(microService, folder)); }
		public static void InstanceEndpointChanged(Guid endpoint) { Notify(nameof(InstanceEndpointChanged), new InstanceEndpointEventArgs(endpoint)); }
		public static void InstanceEndpointRemoved(Guid endpoint) { Notify(nameof(InstanceEndpointRemoved), new InstanceEndpointEventArgs(endpoint)); }
		public static void EnvironmentUnitChanged(Guid environmentUnit) { Notify(nameof(EnvironmentUnitChanged), new EnvironmentUnitEventArgs(environmentUnit)); }
		public static void EnvironmentUnitRemoved(Guid environmentUnit) { Notify(nameof(EnvironmentUnitRemoved), new EnvironmentUnitEventArgs(environmentUnit)); }
		public static void ResourceGroupChanged(Guid resourceGroup) { Notify(nameof(ResourceGroupChanged), new ResourceGroupEventArgs(resourceGroup)); }
		public static void ResourceGroupRemoved(Guid resourceGroup) { Notify(nameof(ResourceGroupRemoved), new ResourceGroupEventArgs(resourceGroup)); }
		public static void SettingChanged(Guid resourceGroup, string name) { Notify(nameof(SettingChanged), new SettingEventArgs(resourceGroup, name)); }
		public static void SettingRemoved(Guid resourceGroup, string name) { Notify(nameof(SettingRemoved), new SettingEventArgs(resourceGroup, name)); }
		public static void MicroServiceChanged(Guid microService) { Notify(nameof(MicroServiceChanged), new MicroServiceEventArgs(microService)); }
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

		public static void UserChanged(Guid user) { Notify(nameof(UserChanged), new UserEventArgs(user)); }
		public static void RoleChanged(Guid role) { Notify(nameof(RoleChanged), new RoleEventArgs(role)); }
		public static void MembershipAdded(Guid user, Guid role) { Notify(nameof(MembershipAdded), new MembershipEventArgs(user, role)); }
		public static void MembershipRemoved(Guid user, Guid role) { Notify(nameof(MembershipRemoved), new MembershipEventArgs(user, role)); }
		public static void PermissionAdded(Guid resourceGroup, Guid evidence, string schema, string claim, string primaryKey) { Notify(nameof(PermissionAdded), new PermissionEventArgs(resourceGroup, evidence, schema, claim, primaryKey)); }
		public static void PermissionChanged(Guid resourceGroup, Guid evidence, string schema, string claim, string primaryKey) { Notify(nameof(PermissionChanged), new PermissionEventArgs(resourceGroup, evidence, schema, claim, primaryKey)); }
		public static void PermissionRemoved(Guid resourceGroup, Guid evidence, string schema, string claim, string primaryKey) { Notify(nameof(PermissionRemoved), new PermissionEventArgs(resourceGroup, evidence, schema, claim, primaryKey)); }
		public static void ComponentChanged(Guid microService, Guid folder, Guid component) { Notify(nameof(ComponentChanged), new ComponentEventArgs(microService, folder, component)); }
		public static void ComponentRemoved(Guid microService, Guid folder, Guid component) { Notify(nameof(ComponentRemoved), new ComponentEventArgs(microService, folder, component)); }
		public static void ComponentAdded(Guid microService, Guid folder, Guid component) { Notify(nameof(ComponentAdded), new ComponentEventArgs(microService, folder, component)); }
		/*
		 * Configuration
		 */
		public static void ConfigurationChanged(Guid microService, Guid configuration, string category) { Notify(nameof(ConfigurationChanged), new ConfigurationEventArgs(microService, configuration, category)); }
		public static void ConfigurationRemoved(Guid microService, Guid configuration, string category) { Notify(nameof(ConfigurationRemoved), new ConfigurationEventArgs(microService, configuration, category)); }
		public static void ConfigurationAdded(Guid microService, Guid configuration, string category) { Notify(nameof(ConfigurationAdded), new ConfigurationEventArgs(microService, configuration, category)); }

		public static void ScriptChanged(Guid microService, Guid sourceCode) { Notify(nameof(ScriptChanged), new ScriptChangedEventArgs(microService, sourceCode)); }
	}
}