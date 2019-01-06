namespace TomPIT.Data.Sql
{
	public abstract class DatabaseConnection : ConnectionBase
	{
		private ReliableSqlConnection _connection = null;
		private IDataTransaction _transaction = null;

		protected void SetConnection(IDataTransaction transaction)
		{
			if (transaction == null)
				return;

			_transaction = transaction;
		}

		protected virtual IDataTransaction CurrentTransaction { get { return _transaction; } }

		public override ReliableSqlConnection Connection
		{
			get
			{
				if (CurrentTransaction != null)
					return CurrentTransaction.Connection;
				else
				{
					if (_connection == null)
						_connection = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.NoRetry);

					return _connection;
				}
			}
		}
	}
}