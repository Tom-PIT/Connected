using System.Data;
using System.Data.SqlClient;
using TomPIT.Data;
using TomPIT.Data.Sql;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class Synchronizer
	{
		private SqlCommand _command = null;
		private ReliableSqlConnection _con = null;
		private IDbTransaction _transaction = null;

		public Synchronizer(string connectionString, IModelSchema schema)
		{
			ConnectionString = connectionString;
			Schema = schema;
		}

		private string ConnectionString { get; }
		private IModelSchema Schema { get; }

		public void Execute()
		{
		}

		private ReliableSqlConnection Connection
		{
			get
			{
				if (_con == null)
					_con = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

				return _con;
			}
		}

		private void Begin()
		{
			Connection.Open();
			_transaction = Connection.BeginTransaction();
		}

		private void Commit()
		{
			_transaction.Commit();
		}

		private void Rollback()
		{
			_transaction.Rollback();
		}

		private void Close()
		{
			Connection.Close();
		}
	}
}
