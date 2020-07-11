using System;
using System.Collections.Generic;
using TomPIT.Development;

namespace TomPIT.Design
{
	public interface IDesignService
	{
		IRepositories Repositories { get; }
		IServiceBindings Bindings { get; }
		IVersionControl VersionControl { get; }
		IComponentModel Components { get; }
		IDesignSearch Search { get; }

		void Pull(IServiceBinding binding);
		void Pull(IServiceBinding binding, List<Guid> components);
		void Push(IServiceBinding binding);
		void Push(IServiceBinding binding, List<Guid> components);
	}
}
