using System.Collections.Immutable;
using TomPIT.Configuration;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class SettingController : ISettingController
	{
		public ImmutableList<ISetting> Query()
		{
			return DataModel.Settings.Query();
		}

		public ISetting Select(string name, string nameSpace, string type, string primaryKey)
		{
			return DataModel.Settings.Select(name, nameSpace, type, primaryKey);
		}
	}
}
