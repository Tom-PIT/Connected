using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	internal class SettingService : ClientRepository<ISetting, string>, ISettingService, ISettingNotification
	{
		public event SettingChangedHandler SettingChanged;

		public SettingService(ITenant tenant) : base(tenant, "setting")
		{

		}

		public T GetValue<T>(string name, string type, string primaryKey)
		{
			var s = Select(name, type, primaryKey);

			if (Types.TryConvert(s.Value, out T vr))
				return vr;

			return default(T);
		}

		public List<ISetting> Query()
		{
			var u = Tenant.CreateUrl("Setting", "Query");

			return Tenant.Get<List<Setting>>(u).ToList<ISetting>();
		}

		public ISetting Select(string name, string type, string primaryKey)
		{
			var key = GenerateKey(name, type, primaryKey);
			var r = Get(key);

			if (r != null)
				return r;

			r = Tenant.Post<Setting>(Tenant.CreateUrl("Setting", "Select"), new
			{
				Name = name,
				Type = type,
				PrimaryKey = primaryKey
			});

			if (r == null)
				r = new Setting();

			Set(key, r);

			return r;
		}

		public void NotifyChanged(object sender, SettingEventArgs e)
		{
			Refresh(GenerateKey(e.Name, e.Type, e.PrimaryKey));
			SettingChanged?.Invoke(sender, e);
		}

		public void NotifyRemoved(object sender, SettingEventArgs e)
		{
			Remove(GenerateKey(e.Name, e.Type, e.PrimaryKey));
			SettingChanged?.Invoke(sender, e);
		}

		public void Update(string name, string type, string primaryKey, object value)
		{
			Tenant.Post(Tenant.CreateUrl("SettingManagement", "Update"), new
			{
				Name = name,
				Type = type,
				PrimaryKey = primaryKey,
				Value = value
			});

			NotifyChanged(this, new SettingEventArgs(name, type, primaryKey));
		}
	}
}