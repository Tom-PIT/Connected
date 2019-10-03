namespace TomPIT.Middleware
{
	public enum DistributedOperationTarget
	{
		Distributed = 1,
		InProcess = 2
	}

	public interface IDistributedOperation : IMiddlewareObject
	{
		bool Cancel { get; set; }
		DistributedOperationTarget OperationTarget { get; }

		IMiddlewareCallback Callback { get; }
	}
}
