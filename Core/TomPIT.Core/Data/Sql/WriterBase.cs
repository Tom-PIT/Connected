using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public abstract class WriterBase<T> : DatabaseCommand
	{
		protected override string ConnectionKey { get { return "sys"; } }

		public void ChangeCommandText(string commandText, CommandType type)
		{
			CommandText = commandText;
			CommandType = type;
		}

		public WriterBase(IDataTransaction transaction)
				: base()
		{
			SetConnection(transaction);
		}

		public WriterBase(string commandText)
				: base(commandText)
		{

		}

		public WriterBase(string commandText, IDataTransaction transaction)
				: base(commandText)
		{
			SetConnection(transaction);
		}

		public WriterBase(string commandText, System.Data.CommandType type)
				: base(commandText)
		{
			CommandType = type;
		}

		public WriterBase(string commandText, System.Data.CommandType type, IDataTransaction transaction)
				: base(commandText)
		{
			CommandType = type;
			SetConnection(transaction);
		}

		public static bool Delete(string procedureName, int recordId)
		{
			var p = new Writer(procedureName);

			p.CreateParameter("@id", recordId);

			try
			{
				p.Execute();

				return true;
			}
			catch (SqlException ex)
			{
				if (ex.Number == 547)
					return false;
				else
					throw ex;
			}
		}

		public static void ExecuteParameterless(string procedureName)
		{
			new Writer(procedureName).Execute();
		}

		public static void ExecuteWithId(string procedureName, int id)
		{
			var p = new Writer(procedureName);

			p.CreateParameter("@id", id);

			p.Execute();
		}

		public static bool Delete(string procedureName, int recordId, IDataTransaction transaction)
		{
			var p = new Writer(procedureName);

			p.CreateParameter("@id", recordId);

			try
			{
				p.Execute(transaction);

				return true;
			}
			catch (SqlException ex)
			{
				if (ex.Number == 547)
					return false;
				else
					throw ex;
			}
		}

		public virtual void Complete()
		{
			if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
			{
				if (!InTransaction)
					Connection.Close();
			}
		}

		public virtual void Prepare()
		{
			Prepared = Connection.CreateCommand();

			Prepared.CommandText = CommandText;
			Prepared.CommandType = CommandType;
			Prepared.CommandTimeout = 300;

			if (CurrentTransaction != null)
				Prepared.Transaction = CurrentTransaction.ActiveTransaction as SqlTransaction;

			foreach (SqlParameter p in Parameters)
				Prepared.Parameters.Add(p);

			if (CommandType == CommandType.StoredProcedure)
			{
				SqlParameter rv = new SqlParameter("@RETURN_VALUE", DBNull.Value)
				{
					Direction = ParameterDirection.ReturnValue
				};

				Prepared.Parameters.Add(rv);
			}

			if (!InTransaction)
				Connection.Open();
		}

		public virtual void Execute()
		{
			SqlCommand command = null;

			if (Prepared != null)
				command = Prepared;
			else
				command = Connection.CreateCommand();

			try
			{
				if (Prepared == null)
				{
					command.CommandText = CommandText;
					command.CommandType = CommandType;
					command.CommandTimeout = 300;

					if (CurrentTransaction != null)
						command.Transaction = CurrentTransaction.ActiveTransaction as SqlTransaction;

					foreach (var p in Parameters)
						command.Parameters.Add(p);

					if (CommandType == CommandType.StoredProcedure)
					{
						var rv = new SqlParameter("@RETURN_VALUE", DBNull.Value);

						rv.Direction = System.Data.ParameterDirection.ReturnValue;

						command.Parameters.Add(rv);
					}

					if (!InTransaction)
						Connection.Open();
				}

				RowsAffected = Connection.ExecuteCommand(command);

				if (command.Parameters.Contains("@RETURN_VALUE"))
				{
					object result = command.Parameters["@RETURN_VALUE"].Value;

					if (result != null && result != DBNull.Value)
						Result = ParseResult(result);
				}
			}
			finally
			{
				if (Connection.State != ConnectionState.Closed)
				{
					if (!InTransaction)
					{
						if (Prepared == null)
							Connection.Close();
					}
				}
			}
		}

		protected abstract T ParseResult(object result);

		public virtual void Execute(IDataTransaction transaction)
		{
			SetConnection(transaction);

			Execute();
		}

		public int RowsAffected { get; private set; }
		public T Result { get; set; } = default(T);
		protected bool InTransaction { get { return CurrentTransaction != null; } }
		private CommandType CommandType { get; set; } = CommandType.StoredProcedure;
		private SqlCommand Prepared { get; set; }
	}
}