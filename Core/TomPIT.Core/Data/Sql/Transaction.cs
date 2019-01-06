using System.Data;

namespace TomPIT.Data.Sql
{
	public class Transaction : ConnectionBase, IDataTransaction
	{
		private ReliableSqlConnection _connection = null;
		protected override string ConnectionKey { get { return "sys"; } }
		private IDbTransaction _transaction = null;
		private bool _active = false;

		public Transaction()
		{

		}

		public void Begin()
		{
			Connection.Open();

			_transaction = Connection.BeginTransaction();
			_active = true;
		}


		public void Commit()
		{
			if (_transaction != null && _active)
			{
				_transaction.Commit();
				_active = false;
			}

			if (Connection.State != System.Data.ConnectionState.Closed)
				Connection.Close();
		}

		public void Rollback()
		{
			if (_transaction != null && _active)
			{
				_transaction.Rollback();
				_active = false;
			}

			if (Connection.State != System.Data.ConnectionState.Closed)
				Connection.Close();
		}

		public IDbTransaction ActiveTransaction
		{
			get { return _transaction; }
		}

		public override ReliableSqlConnection Connection
		{
			get
			{
				if (_connection == null)
					_connection = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

				return _connection;
			}
		}
	}
}