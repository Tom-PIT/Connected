using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Configuration
{
	internal class SettingService : ClientRepository<ISetting, string>, ISettingService, ISettingNotification
	{
		public event SettingChangedHandler SettingChanged;

		public SettingService(ISysConnection connection) : base(connection, "setting")
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
			var u = Connection.CreateUrl("Setting", "Query")
				.AddParameter("resourceGroup", resourceGroup);

			return Connection.Get<List<Setting>>(u).ToList<ISetting>();
		}

		public ISetting Select(Guid resourceGroup, string name)
		{
			var key = GenerateKey(resourceGroup, name);
			var r = Get(key);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("Setting", "Select")
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("name", name);

			r = Connection.Get<Setting>(u);

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