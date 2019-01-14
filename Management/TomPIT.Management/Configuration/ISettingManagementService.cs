using System;

namespace TomPIT.Configuration
{
	public interface ISettingManagementService
	{
		void Update(Guid resourceGroup, string name, string value, bool visible, DataType dataType, string tags);
		void Delete(Guid resourceGroup, string name);
	}
}
