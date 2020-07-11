using System;
using System.Collections.Generic;
using TomPIT.Development;

namespace TomPIT.Design
{
	public interface IServiceBindings
	{
		List<IServiceBinding> QueryActive();
		IServiceBinding Select(Guid service, string repository);
		void Update(Guid service, string repository, long commit, DateTime date, bool active);
		void Delete(Guid service, string repository);
	}
}
