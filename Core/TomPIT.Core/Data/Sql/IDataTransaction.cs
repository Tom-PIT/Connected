namespace TomPIT.Data.Sql
{
	public interface IDataTransaction
	{
		void Begin();
		void Commit();
		void Rollback();

		ReliableSqlConnection Connection { get; }
		System.Data.IDbTransaction ActiveTransaction { get; }
	}
}