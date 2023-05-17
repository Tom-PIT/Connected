using System;

namespace TomPIT.Proxy.Management
{
	public interface IRoleManagementController
	{
		Guid Insert(string name);
		void Update(Guid token, string name);
		void Delete(Guid token);
	}
}
