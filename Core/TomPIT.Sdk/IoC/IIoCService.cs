using System.Collections.Generic;

namespace TomPIT.IoC
{
	public interface IIoCService
	{
		T CreateMiddleware<T>() where T : class;
		T CreateMiddleware<T, A>(A arguments) where T : class;
		List<IIoCEndpointMiddleware> CreateEndpoints<A>(IIoCOperationMiddleware sender, A e);
		bool HasEndpoints<A>(IIoCOperationMiddleware sender, A e);
	}
}
