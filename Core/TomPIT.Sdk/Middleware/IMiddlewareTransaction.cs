namespace TomPIT.Middleware
{
	public enum MiddlewareTransactionState
	{
		Active = 1,
		Committing = 2,
		Reverting = 3,
		Completed = 4
	}
	public interface IMiddlewareTransaction
	{
		MiddlewareTransactionState State { get; }
	}
}
