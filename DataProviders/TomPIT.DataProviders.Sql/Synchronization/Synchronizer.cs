using System;
using System.Collections.Generic;
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

		public Synchronizer(string connectionString, IModelSchema schema, List<IModelOperationSchema> procedures)
		{
			ConnectionString = connectionString;
			Schema = schema;
			Procedures = procedures;
		}

		private List<IModelOperationSchema> Procedures { get; }
		private string ConnectionString { get; }
		private IModelSchema Schema { get; }

		public void Execute()
		{
			try
			{
				Begin();
				Synchronize();
				Commit();
			}
			catch
			{
				Rollback();
			}
			finally
			{
				Close();
			}
		}

		private void Synchronize()
		{
			new SchemaSynchronizer(Command, Schema).Execute();

			if (string.IsNullOrWhiteSpace(Schema.Type) || string.Compare(Schema.Type, "Table", true) == 0)
				new TableSynchronizer(Command, Schema).Execute();
			else if (string.Compare(Schema.Type, "View", true) == 0)
				new ViewSynchronizer(Command, Schema).Execute();
			else
				throw new NotSupportedException();

			foreach (var procedure in Procedures)
				new ProcedureSynchronizer(Command, Schema, procedure.Text).Execute();
		}

		private SqlCommand Command
		{
			get
			{
				if (_command == null)
				{
					_command = Connection.CreateCommand();
					_command.CommandType = CommandType.Text;
					_command.Transaction = Transaction;
				}

				return _command;
			}
		}

		private SqlTransaction Transaction => _transaction as SqlTransaction;

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
