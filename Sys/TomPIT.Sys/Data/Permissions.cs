using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Environment;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class Permissions : SynchronizedRepository<IPermission, string>
	{
		public Permissions(IMemoryCache container) : base(container, "permission")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Query();

			foreach (var i in ds)
				Set(GenerateKey(i.Evidence.ToString(), i.Schema, i.Claim, i.PrimaryKey), i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var r = Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Select(new Guid(tokens[0]), tokens[1], tokens[2], tokens[3]);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IPermission Select(Guid evidence, string schema, string claim, string primaryKey)
		{
			return Get(GenerateKey(evidence.ToString(), schema, claim, primaryKey),
				(f) =>
				{
					return Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Select(evidence, schema, claim, primaryKey);
				});
		}

		public List<IPermission> Query(List<string> resourceGroups)
		{
			return Query(resourceGroups.ToResourceGroupList());
		}

		public List<IPermission> Query(List<Guid> resourceGroups)
		{
			return Where(f => resourceGroups.Any(t => t == f.ResourceGroup));
		}

		public List<IPermission> Query(string primaryKey)
		{
			return Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);
		}

		public List<IPermission> Query() { return All(); }

		public void Insert(Guid resourceGroup, Guid evidence, string schema, string claim, string descriptor, string primaryKey, PermissionValue value, string component)
		{
			IResourceGroup rg = null;

			if (resourceGroup != Guid.Empty)
			{
				rg = DataModel.ResourceGroups.Select(resourceGroup);

				if (rg == null)
					throw new SysException(SR.ErrResourceGroupNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Insert(rg, evidence, schema, claim, descriptor, primaryKey, value, component);

			var key = GenerateKey(evidence.ToString(), schema, claim, primaryKey);

			Refresh(key);
			CachingNotifications.PermissionAdded(resourceGroup, evidence, schema, claim, primaryKey);
		}

		public void Update(Guid evidence, string schema, string claim, string primaryKey, PermissionValue value)
		{
			var p = Select(evidence, schema, claim, primaryKey);

			if (p == null)
				throw new SysException(SR.ErrPermissionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Update(p, value);

			var key = GenerateKey(evidence.ToString(), schema, claim, primaryKey);

			Refresh(key);
			CachingNotifications.PermissionChanged(p.ResourceGroup, evidence, schema, claim, primaryKey);
		}

		public void Delete(Guid evidence, string schema, string claim, string primaryKey)
		{
			var p = Select(evidence, schema, claim, primaryKey);

			if (p == null)
				throw new SysException(SR.ErrPermissionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Delete(p);

			var key = GenerateKey(evidence.ToString(), schema, claim, primaryKey);

			Remove(key);
			CachingNotifications.PermissionRemoved(p.ResourceGroup, evidence, schema, claim, primaryKey);
		}

		public void Reset(string claim, string schema, string primaryKey)
		{
			List<IPermission> permissions = null;

			if (!string.IsNullOrWhiteSpace(claim))
				permissions = Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0 && string.Compare(f.Claim, claim, true) == 0 && string.Compare(f.Schema, schema, true) == 0);
			else
				permissions = Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);

			foreach (var i in permissions)
				Delete(i.Evidence, i.Schema, i.Claim, i.PrimaryKey);
		}
	}
}