namespace TomPIT.Middleware
{
	public interface IMiddlewareOperation : IMiddlewareComponent
	{
		IMiddlewareTransaction BeginTransaction();
		IMiddlewareTransaction BeginTransaction(string name);

		void Commit();
		void Rollback();
	}
}
