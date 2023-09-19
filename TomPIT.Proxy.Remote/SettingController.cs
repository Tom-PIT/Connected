using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Configuration;

namespace TomPIT.Proxy.Remote
{
	internal class SettingController : ISettingController
	{
		private const string Controller = "Setting";
		public T GetValue<T>(string name, string nameSpace, string type, string primaryKey)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<ISetting> Query()
		{
			return Connection.Get<List<Setting>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<ISetting>();
		}

		public ISetting Select(string name, string nameSpace, string type, string primaryKey)
		{
			return Connection.Post<Setting>(Connection.CreateUrl(Controller, "Select"), new
			{
				Name = name,
				Type = type,
				NameSpace = nameSpace,
				PrimaryKey = primaryKey
			});
		}
	}
}
