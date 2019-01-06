using System;
using System.Collections.Generic;
using TomPIT.Caching;
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
				Set(GenerateKey(i.Evidence.AsString(), i.Schema, i.Claim, i.PrimaryKey), i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var r = Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Select(tokens[0].AsGuid(), tokens[1], tokens[2], tokens[3]);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IPermission Select(Guid evidence, string schema, string claim, string primaryKey)
		{
			return Get(GenerateKey(evidence.AsString(), schema, claim, primaryKey),
				(f) =>
				{
					return Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Select(evidence, schema, claim, primaryKey);
				});
		}

		public List<IPermission> Query(string primaryKey)
		{
			return Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);
		}

		public List<IPermission> Query() { return All(); }

		public void Insert(Guid evidence, string schema, string claim, string descriptor, string primaryKey, PermissionValue value)
		{
			Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Insert(evidence, schema, claim, descriptor, primaryKey, value);

			var key = GenerateKey(evidence.AsString(), schema, claim, primaryKey);

			Refresh(key);
			NotificationHubs.PermissionAdded(evidence, schema, claim, primaryKey);
		}

		public void Update(Guid evidence, string schema, string claim, string primaryKey, PermissionValue value)
		{
			var p = Select(evidence, schema, claim, primaryKey);

			if (p == null)
				throw new SysException(SR.ErrPermissionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Update(p, value);

			var key = GenerateKey(evidence.AsString(), schema, claim, primaryKey);

			Refresh(key);
			NotificationHubs.PermissionChanged(evidence, schema, claim, primaryKey);
		}

		public void Delete(Guid evidence, string schema, string claim, string primaryKey)
		{
			var p = Select(evidence, schema, claim, primaryKey);

			if (p == null)
				throw new SysException(SR.ErrPermissionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Permissions.Delete(p);

			var key = GenerateKey(evidence.AsString(), schema, claim, primaryKey);

			Remove(key);
			NotificationHubs.PermissionRemoved(evidence, schema, claim, primaryKey);
		}

		public void Reset(string primaryKey)
		{
			var p = Where(f => string.Compare(f.PrimaryKey, primaryKey, true) == 0);

			foreach (var i in p)
				Delete(i.Evidence, i.Schema, i.Claim, i.PrimaryKey);
		}
	}
}