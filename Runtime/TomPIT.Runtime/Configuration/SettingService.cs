using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;

namespace TomPIT.Configuration
{
	internal class SettingService : ContextCacheRepository<ISetting, string>, ISettingService, ISettingNotification
	{
		public event SettingChangedHandler SettingChanged;

		public SettingService(ISysContext server) : base(server, "setting")
		{

		}

		public T GetValue<T>(Guid resourceGroup, string name)
		{
			var s = Select(resourceGroup, name);

			if (Types.TryConvert(s.Value, out T vr))
				return vr;

			return default(T);
		}

		public List<ISetting> Query(Guid resourceGroup)
		{
			var u = Server.CreateUrl("Setting", "Query")
				.AddParameter("resourceGroup", resourceGroup);

			return Server.Connection.Get<List<Setting>>(u).ToList<ISetting>();
		}

		public ISetting Select(Guid resourceGroup, string name)
		{
			var key = GenerateKey(resourceGroup, name);
			var r = Get(key);

			if (r != null)
				return r;

			var u = Server.CreateUrl("Setting", "Select")
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("name", name);

			r = Server.Connection.Get<Setting>(u);

			if (r == null)
				r = new Setting();

			Set(key, r);

			return r;
		}

		public void NotifyChanged(object sender, SettingEventArgs e)
		{
			SettingChanged?.Invoke(sender, e);
		}
	}
}