using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Middleware;
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
			List<Folder> ds = null;

			if (Tenant.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
			{
				var u = Tenant.CreateUrl("Folder", "Query");
				ds = Tenant.Get<List<Folder>>(u);
			}
			else
			{
				var u = Tenant.CreateUrl("Folder", "QueryForResourceGroups");
				var e = new JArray();

				foreach (var i in Tenant.GetService<IResourceGroupService>().Query())
					e.Add(i.Name.ToString());

				ds = Tenant.Post<List<Folder>>(u, e);
			}

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Tenant.CreateUrl("Folder", "SelectByToken")
				.AddParameter("token", id);

			var d = Tenant.Get<Folder>(u);

			if (d == null)
				return;

			Set(d.Token, d, TimeSpan.Zero);
		}

		public IFolder Select(Guid token)
		{
			return Get(token);
		}

		public List<IFolder> Query(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public List<IFolder> Query(Guid microService, Guid parent)
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

			var u = Tenant.CreateUrl("Folder", "QueryForMicroService");
			var e = new JObject
			{
				{"microService", microService }
			};

			var items = Tenant.Post<List<Folder>>(u, e);

			foreach (var i in items)
				Set(i.Token, i, TimeSpan.Zero);
		}
	}
}
