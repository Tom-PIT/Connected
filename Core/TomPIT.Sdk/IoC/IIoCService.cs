using System.Collections.Generic;
using TomPIT.ComponentModel.IoC;

namespace TomPIT.IoC
{
	public interface IIoCService
	{
		R Invoke<R>(IIoCOperation operation);
		R Invoke<R>(IIoCOperation operation, object e);

		void Invoke(IIoCOperation operation);
		void Invoke(IIoCOperation operation, object e);
		List<IIoCEndpointMiddleware> CreateEndpoints(IIoCOperation operation, object e);
		bool HasEndpoints(IIoCOperation sender, object e);
	}
}
