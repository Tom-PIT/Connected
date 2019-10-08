using System;
using System.Collections.Generic;

namespace TomPIT.IoC
{
	public interface IIoCService
	{
		T CreateMiddleware<T>(Guid microService) where T : class;
		T CreateMiddleware<T, A>(Guid microService, A arguments) where T : class;
		List<IIoCEndpointMiddleware> CreateEndpoints<A>(IIoCContainerMiddleware sender, A e);
		bool HasEndpoints<A>(IIoCContainerMiddleware sender, A e);
	}
}
