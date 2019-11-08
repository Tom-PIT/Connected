using System.Collections.Generic;

namespace TomPIT.Middleware
{
	public enum DistributedOperationTarget
	{
		Distributed = 1,
		InProcess = 2
	}

	public interface IDistributedOperation : IMiddlewareObject
	{
		List<IOperationResponse> Responses { get; }
		DistributedOperationTarget OperationTarget { get; }

		IMiddlewareCallback Callback { get; }
		void Invoke();
	}
}
