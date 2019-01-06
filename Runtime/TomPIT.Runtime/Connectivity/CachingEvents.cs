using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Compilers;
using TomPIT.ComponentModel;
using TomPIT.Environment;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Net
{
	internal class CachingEvents
	{
		public CachingEvents(ISysContext context, HubConnection connection)
		{
			Connection = connection;
			Context = context;
		}

		private HubConnection Connection { get; }
		private ISysContext Context { get; }

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
		}

		private void HookEnvironmentUnits()
		{
			Connection.On<MessageEventArgs<EnvironmentUnitEventArgs>>("EnvironmentUnitChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
					n.NotifyChanged(Context, e.Args);
			});

			Connection.On<MessageEventArgs<EnvironmentUnitEventArgs>>("EnvironmentUnitRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
					n.NotifyRemoved(Context, e.Args);
			});
		}

		private void HookInstances()
		{
			Connection.On<MessageEventArgs<InstanceEndpointEventArgs>>("InstanceEndpointChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
					n.NotifyChanged(Context, e.Args);
			});

			Connection.On<MessageEventArgs<InstanceEndpointEventArgs>>("InstanceEndpointRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
					n.NotifyRemoved(Context, e.Args);
			});
		}

		private void HookMicroServices()
		{
			Connection.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyChanged(Context, e.Args);
			});

			Connection.On<MessageEventArgs<MicroServiceEventArgs>>("MicroServiceRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyRemoved(Context, e.Args);
			});
		}

		private void HookSecurity()
		{
			Connection.On<MessageEventArgs<MembershipEventArgs>>("MembershipAdded", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyMembershipAdded(Context, e.Args);
			});

			Connection.On<MessageEventArgs<MembershipEventArgs>>("MembershipRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyMembershipRemoved(Context, e.Args);
			});

			Connection.On<MessageEventArgs<PermissionEventArgs>>("PermissionAdded", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionAdded(Context, e.Args);
			});

			Connection.On<MessageEventArgs<PermissionEventArgs>>("PermissionRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionRemoved(Context, e.Args);
			});

			Connection.On<MessageEventArgs<PermissionEventArgs>>("PermissionChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IAuthorizationService>() is IAuthorizationNotification n)
					n.NotifyPermissionChanged(Context, e.Args);
			});
		}

		private void HookConfiguration()
		{
			Connection.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationAdded", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyAdded(Context, e.Args);
			});

			Connection.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyChanged(Context, e.Args);
			});

			Connection.On<MessageEventArgs<ConfigurationEventArgs>>("ConfigurationRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IComponentService>() is IComponentNotification n)
					n.NotifyRemoved(Context, e.Args);
			});

			Connection.On<MessageEventArgs<ScriptChangedEventArgs>>("ScriptChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<ICompilerService>() is ICompilerNotification n)
					n.NotifyChanged(Context, e.Args);
			});
		}

		private void HookUsers()
		{
			Connection.On<MessageEventArgs<UserEventArgs>>("UserChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IUserService>() is IUserNotification n)
					n.NotifyChanged(Context, e.Args);
			});
		}

		private void HookRoles()
		{
			Connection.On<MessageEventArgs<RoleEventArgs>>("RoleChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IRoleService>() is IRoleNotification n)
					n.NotifyChanged(Context, e.Args);
			});
		}

		private void HookFeatures()
		{
			Connection.On<MessageEventArgs<FeatureEventArgs>>("FeatureChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IFeatureService>() is IFeatureNotification n)
					n.NotifyChanged(Context, e.Args);
			});

			Connection.On<MessageEventArgs<FeatureEventArgs>>("FeatureRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IFeatureService>() is IFeatureNotification n)
					n.NotifyChanged(Context, e.Args);
			});
		}

		private void HookBlobs()
		{
			Connection.On<MessageEventArgs<BlobEventArgs>>("BlobChanged", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyChanged(Context, e.Args);
			});

			Connection.On<MessageEventArgs<BlobEventArgs>>("BlobAdded", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyAdded(Context, e.Args);
			});

			Connection.On<MessageEventArgs<BlobEventArgs>>("BlobRemoved", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyRemoved(Context, e.Args);
			});

			Connection.On<MessageEventArgs<BlobEventArgs>>("BlobCommitted", (e) =>
			{
				Connection.InvokeAsync("Confirm", e.Message);

				if (Context.GetService<IStorageService>() is IStorageNotification n)
					n.NotifyCommitted(Context, e.Args);
			});
		}
	}
}
