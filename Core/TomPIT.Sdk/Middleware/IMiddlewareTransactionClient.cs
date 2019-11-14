namespace TomPIT.Middleware
{
	public interface IMiddlewareTransactionClient
	{
		void CommitTransaction();
		void RollbackTransaction();
	}
}
