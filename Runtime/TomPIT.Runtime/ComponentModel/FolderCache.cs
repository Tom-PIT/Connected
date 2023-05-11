using System;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	internal class FolderCache : SynchronizedClientRepository<IFolder, Guid>
	{
		public FolderCache(ITenant tenant) : base(tenant, "folder")
		{
		}

		protected override void OnInitializing()
		{
			ImmutableList<IFolder> ds;

			if (Tenant.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				ds = Instance.SysProxy.Folders.Query();
			else
				ds = Instance.SysProxy.Folders.Query(Tenant.GetService<IResourceGroupService>().Query().Select(f => f.Name).ToList());

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var d = Instance.SysProxy.Folders.Select(id);

			if (d is null)
				return;

			Set(d.Token, d, TimeSpan.Zero);
		}

		public IFolder Select(Guid token)
		{
			return Get(token);
		}

		public ImmutableList<IFolder> Query(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public ImmutableList<IFolder> Query(Guid microService, Guid parent)
		{
			return Where(f => f.MicroService == microService && f.Parent == parent);
		}

		public void Reload(Guid id)
		{
			Refresh(id);
		}

		public void Delete(Guid id)
		{
			Remove(id);
		}

		public void RefreshMicroService(Guid microService)
		{
			var ds = All();

			foreach (var i in ds)
			{
				if (i.MicroService == microService)
					Remove(i.Token);
			}

			var items = Instance.SysProxy.Folders.Query(microService);

			foreach (var i in items)
				Set(i.Token, i, TimeSpan.Zero);
		}
	}
}
