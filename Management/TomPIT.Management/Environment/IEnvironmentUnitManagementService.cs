using System;

namespace TomPIT.Environment
{
	public interface IEnvironmentUnitManagementService
	{
		Guid Insert(string name, Guid parent, int ordinal);
		void Update(Guid unit, string name, Guid parent, int ordinal);
		void Delete(Guid unit);
	}
}
