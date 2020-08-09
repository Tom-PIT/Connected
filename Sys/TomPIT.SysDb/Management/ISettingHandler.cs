using System.Collections.Generic;
using TomPIT.Configuration;

namespace TomPIT.SysDb.Management
{
	public interface ISettingHandler
	{
		void Insert(string name, string nameSpace, string type, string primaryKey, string value);
		void Update(ISetting setting, string value);
		List<ISetting> Query();
		ISetting Select(string name, string nameSpace, string type, string primaryKey);
		void Delete(ISetting setting);
	}
}
