using System;
using System.Collections.Immutable;
using TomPIT.Api.Storage;
using TomPIT.Caching;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Components
{
	public class ResourceGroupsModel : SynchronizedRepository<IServerResourceGroup, Guid>
	{
		public static readonly Guid DefaultResourceGroup = new Guid("E14372D117CD48D6BC29D57C397AF87C");

		public ResourceGroupsModel(IMemoryCache container) : base(container, "resourceGroup")
		{
		}

		public IServerResourceGroup Select(string name)
		{
			return Get(f => string.Compare(f.Name, name, true) == 0);
		}

		public IServerResourceGroup Select(Guid token)
		{
			return Get(token,
				 (f) =>
				 {
					 f.Duration = TimeSpan.Zero;

					 return Shell.GetService<IDatabaseService>().Proxy.Environment.SelectResourceGroup(token);
				 });
		}

		public ImmutableList<IServerResourceGroup> Query()
		{
			return All();
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Environment.QueryResourceGroups();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);

			if (ds.Count == 0)
				InsertDefault(DefaultResourceGroup, "Default", Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Token, string.Empty);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectResourceGroup(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public Guid Insert(string name, Guid storageProvider, string connectionString)
		{
			return Insert(Guid.NewGuid(), name, storageProvider, connectionString);
		}

		private Guid InsertDefault(Guid token, string name, Guid storageProvider, string connectionString)
		{
			Shell.GetService<IDatabaseService>().Proxy.Environment.InsertResourceGroup(token, name, storageProvider, connectionString);

			Refresh(token);

			CachingNotifications.ResourceGroupChanged(token);

			return token;
		}

		public Guid Insert(Guid token, string name, Guid storageProvider, string connectionString)
		{
			var v = new Validator();

			v.Unique(null, name, nameof(IServerResourceGroup.Name), Query());

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			Shell.GetService<IDatabaseService>().Proxy.Environment.InsertResourceGroup(token, name, storageProvider, connectionString);

			Refresh(token);

			CachingNotifications.ResourceGroupChanged(token);

			return token;
		}

		public void Update(Guid token, string name, Guid storageProvider, string connectionString)
		{
			var target = Select(token);

			if (target == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var v = new Validator();

			v.Unique(target, name, nameof(IServerResourceGroup.Name), Query());

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			Shell.GetService<IDatabaseService>().Proxy.Environment.UpdateResourceGroup(target, name, storageProvider, connectionString);

			Refresh(token);

			CachingNotifications.ResourceGroupChanged(token);
		}

		public void Delete(Guid token)
		{
			var target = Select(token);

			if (target == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.DeleteResourceGroup(target);

			Refresh(token);

			CachingNotifications.ResourceGroupRemoved(token);
		}

		public IServerResourceGroup Default
		{
			get
			{
				var r = Get(DefaultResourceGroup);

				if (r != null)
					return r;

				Insert(DefaultResourceGroup, "Default", Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Token, string.Empty);

				return Get(DefaultResourceGroup);
			}
		}
	}
}
