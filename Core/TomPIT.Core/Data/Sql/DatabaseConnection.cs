using System;
using Microsoft.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public abstract class DatabaseConnection : ConnectionBase, IDisposable
	{
		private SqlConnection _connection = null;
		private IDataTransaction _transaction = null;
		private bool disposedValue;

		protected void SetConnection(IDataTransaction transaction)
		{
			if (transaction == null)
				return;

			_transaction = transaction;
		}

		protected virtual IDataTransaction CurrentTransaction { get { return _transaction; } }

		public override SqlConnection Connection
		{
			get
			{
				if (CurrentTransaction != null)
					return CurrentTransaction.Connection;
				else
				{
					if (_connection == null)
						_connection = new SqlConnection(ConnectionString);

					return _connection;
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (_connection != null)
					{
						_connection.Dispose();
						_connection = null;
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}