using System;

namespace TomPIT.ComponentModel
{
	public interface IMicroServiceManagementService
	{
		Guid Insert(string name, Guid resourceGroup, Guid template, MicroServiceStatus status);
		void Update(Guid microService, string name, MicroServiceStatus status, Guid resourceGroup);
		void Delete(Guid microService);

		ListItems<IMicroService> Query(Guid resourceGroup);
	}
}
