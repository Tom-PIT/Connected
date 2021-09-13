using System;
using System.Collections.Immutable;

namespace TomPIT.Security
{
	public interface IRoleService
	{
		ImmutableList<IRole> Query();
		IRole Select(Guid token);
		IRole Select(string name);

		Guid Insert(string name);
		void Update(Guid token, string name);
		void Delete(Guid token);
	}
}
