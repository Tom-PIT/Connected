using Microsoft.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public interface IDataTransaction
	{
		void Begin();
		void Commit();
		void Rollback();

		SqlConnection Connection { get; }
		System.Data.IDbTransaction ActiveTransaction { get; }
	}
}