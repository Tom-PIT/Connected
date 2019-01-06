using System;

namespace TomPIT.Security
{
	public interface IRoleManagementService
	{
		Guid Insert(string name);
		void Update(Guid token, string name);
		void Delete(Guid token);
	}
}
