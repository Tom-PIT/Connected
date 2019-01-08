using System;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Configuration
{
	public interface ISettingManagementService
	{
		void Update(IExecutionContext context, Guid resourceGroup, string name, string value, bool visible, DataType dataType, string tags);
		void Delete(IExecutionContext context, Guid resourceGroup, string name);
	}
}
