using System;
using TomPIT.Runtime;

namespace TomPIT.Configuration
{
	public interface ISettingManagementService
	{
		void Update(IApplicationContext context, Guid resourceGroup, string name, string value, bool visible, DataType dataType, string tags);
		void Delete(IApplicationContext sender, Guid resourceGroup, string name);
	}
}
