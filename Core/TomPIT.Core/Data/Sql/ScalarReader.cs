using System;
using System.Data;
using System.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public class ScalarReader<T> : DatabaseCommand
	{
		private CommandType _commandType = CommandType.StoredProcedure;

		protected override string ConnectionKey { get { return "sys"; } }

		public ScalarReader(string commandText)
			: base(commandText)
		{

		}

		public ScalarReader(string commandText, IDataTransaction transaction)
			: base(commandText)
		{
			SetConnection(transaction);
		}

		public ScalarReader(string commandText, CommandType commandType)
			: base(commandText)
		{
			_commandType = commandType;
		}

		public ScalarReader(string commandText, CommandType commandType, IDataTransaction transaction)
			: base(commandText)
		{
			_commandType = commandType;
			SetConnection(transaction);
		}

		public T ExecuteScalar(T defaultValue)
		{
			SqlCommand command = createCommand();

			try
			{
				prepareConnection();

				object res = Connection.ExecuteCommand<object>(command);

				if (res == null || res == DBNull.Value)
					return defaultValue;
				else
					return (T)Convert.ChangeType(res, typeof(T));
			}
			finally
			{
				finalize(null);
			}
		}

		public T SelectWithId(int id, T defaultValue)
		{
			CreateParameter("@id", id);

			return ExecuteScalar(defaultValue);
		}

		private SqlCommand createCommand()
		{
			SqlCommand command = null;

			command = Connection.CreateCommand();

			command.CommandText = CommandText;

			if (CurrentTransaction != null)
				command.Transaction = CurrentTransaction.ActiveTransaction as SqlTransaction;

			command.CommandType = _commandType;

			foreach (SqlParameter p in Parameters)
				command.Parameters.Add(p);

			return command;
		}

		private void finalize(SqlDataReader rdr)
		{
			if (rdr != null && !rdr.IsClosed)
				rdr.Close();

			if (CurrentTransaction == null && Connection.State != ConnectionState.Closed)
				Connection.Close();
		}

		private void prepareConnection()
		{
			if (CurrentTransaction == null || Connection.State == ConnectionState.Closed)
				Connection.Open();
		}
	}
}