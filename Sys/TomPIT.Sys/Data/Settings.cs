using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Configuration;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class Settings : SynchronizedRepository<ISetting, string>
	{
		public static string TaskFailTreshold = "Task fail treshold";

		public Settings(IMemoryCache container) : base(container, "setting")
		{
		}

		private void Refresh(Guid resourceGroup, string name)
		{
			Refresh(GenerateKey(resourceGroup, name));
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');
			var rgId = tokens[0].AsGuid();
			IResourceGroup rg = null;

			if (rgId != Guid.Empty)
			{
				rg = DataModel.ResourceGroups.Select(tokens[0].AsGuid());

				if (rg == null)
				{
					Remove(id);
					return;
				}
			}

			var r = Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Select(rg, tokens[1]);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public ISetting Select(Guid resourceGroup, string name)
		{
			var key = GenerateKey(resourceGroup, name);

			return Get(key,
				(f) =>
			{
				IResourceGroup r = null;

				if (resourceGroup != Guid.Empty)
				{
					r = DataModel.ResourceGroups.Select(resourceGroup);

					if (r == null)
						throw new SysException(SR.ErrResourceGroupNotFound);
				}

				var d = Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Select(r, name);

				f.AllowNull = true;
				f.Duration = d == null ? TimeSpan.FromMinutes(5) : TimeSpan.Zero;

				Set(key, d);

				return d;
			});
		}

		public List<ISetting> Where(Guid resourceGroup)
		{
			return Where(f => f.ResourceGroup == resourceGroup);
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Query();

			foreach (var i in ds)
				Set(GenerateKey(i.ResourceGroup, i.Name), i, TimeSpan.Zero);
		}

		public void Update(Guid resourceGroup, string name, string value, bool visible, DataType dataType, string tags)
		{
			IResourceGroup r = null;

			if (resourceGroup != Guid.Empty)
			{
				r = DataModel.ResourceGroups.Select(resourceGroup);

				if (r == null)
					throw new SysException(SR.ErrResourceGroupNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Update(r, name, value, visible, dataType, tags);

			Refresh(resourceGroup, name);

			CachingNotifications.SettingChanged(resourceGroup, name);
		}

		public void Delete(Guid resourceGroup, string name)
		{
			IResourceGroup r = null;

			if (resourceGroup != Guid.Empty)
			{
				r = DataModel.ResourceGroups.Select(resourceGroup);

				if (r == null)
					throw new SysException(SR.ErrResourceGroupNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Delete(r, name);

			Remove(GenerateKey(resourceGroup, name));

			CachingNotifications.SettingRemoved(resourceGroup, name);
		}
	}
}
