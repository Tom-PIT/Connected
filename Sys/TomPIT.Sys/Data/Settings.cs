using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Configuration;
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

		private void Refresh(string name, string nameSpace, string type, string primaryKey)
		{
			Refresh(GenerateKey(name, nameSpace, type, primaryKey));
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var r = Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Select(tokens[0], tokens.Length > 1 ? tokens[1] : null, tokens.Length > 2 ? tokens[2] : null, tokens.Length > 3 ? tokens[3] : null);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public ISetting Select(string name, string nameSpace, string type, string primaryKey)
		{
			var key = GenerateKey(name, nameSpace, type, primaryKey);

			return Get(key,
				(f) =>
			{
				var d = Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Select(name, nameSpace, type, primaryKey);

				f.AllowNull = true;
				f.Duration = d == null ? TimeSpan.FromMinutes(5) : TimeSpan.Zero;

				Set(key, d);

				return d;
			});
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Query();

			foreach (var i in ds)
				Set(GenerateKey(i.Name, i.NameSpace, i.Type, i.PrimaryKey), i, TimeSpan.Zero);
		}

		public void Update(string name, string nameSpace, string type, string primaryKey, string value)
		{
			var setting = Select(name, nameSpace, type, primaryKey);

			if (setting == null)
				Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Insert(name, nameSpace, type, primaryKey, value);
			else
				Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Update(setting, value);

			Refresh(name, nameSpace, type, primaryKey);

			CachingNotifications.SettingChanged(name, nameSpace, type, primaryKey);
		}

		public List<ISetting> Query()
		{
			return All();
		}
		public void Delete(string name, string nameSpace, string type, string primaryKey)
		{
			var setting = Select(name, nameSpace, type, primaryKey);

			if (setting == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Management.Settings.Delete(setting);

			Remove(GenerateKey(name, nameSpace, type, primaryKey));

			CachingNotifications.SettingRemoved(name, nameSpace, type, primaryKey);
		}
	}
}
