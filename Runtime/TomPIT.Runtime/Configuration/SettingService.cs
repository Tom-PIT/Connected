using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Configuration
{
	internal class SettingService : ClientRepository<ISetting, string>, ISettingService, ISettingNotification
	{
		public event SettingChangedHandler SettingChanged;

		public SettingService(ITenant tenant) : base(tenant, "setting")
		{

		}

		public T GetValue<T>(string name, string nameSpace, string type, string primaryKey)
		{
			var s = Select(name, nameSpace, type, primaryKey);

			if (Types.TryConvert(s.Value, out T vr))
				return vr;

			return default(T);
		}

		public List<ISetting> Query()
		{
			return Instance.SysProxy.Settings.Query().ToList();
		}

		public ISetting Select(string name, string nameSpace, string type, string primaryKey)
		{
			var key = GenerateKey(name, nameSpace, type, primaryKey);
			var r = Get(key);

			if (r is not null)
				return r;

			r = Instance.SysProxy.Settings.Select(name, nameSpace, type, primaryKey);

			r ??= new Setting();

			Set(key, r);

			return r;
		}

		public void NotifyChanged(object sender, SettingEventArgs e)
		{
			Refresh(GenerateKey(e.Name, e.NameSpace, e.Type, e.PrimaryKey));
			SettingChanged?.Invoke(sender, e);
		}

		public void NotifyRemoved(object sender, SettingEventArgs e)
		{
			Remove(GenerateKey(e.Name, e.NameSpace, e.Type, e.PrimaryKey));
			SettingChanged?.Invoke(sender, e);
		}

		public void Update(string name, string nameSpace, string type, string primaryKey, object value)
		{
			Instance.SysProxy.Management.Settings.Update(name, nameSpace, type, primaryKey, value);
			NotifyChanged(this, new SettingEventArgs(name, nameSpace, type, primaryKey));
		}
	}
}