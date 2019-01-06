using System.Collections.Generic;
using TomPIT.Configuration;
using TomPIT.Environment;

namespace TomPIT.SysDb.Management
{
	public interface ISettingHandler
	{
		void Update(IResourceGroup resourceGroup, string name, string value, bool visible, DataType dataType, string tags);
		List<ISetting> Query();
		ISetting Select(IResourceGroup resourceGroup, string name);
		void Delete(IResourceGroup resourceGroup, string name);
	}
}
