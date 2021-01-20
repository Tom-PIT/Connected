using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public class Reader<T> : DatabaseCommand where T : DatabaseRecord, new()
	{
		private List<T> _result = null;
		private CommandType _commandType = CommandType.StoredProcedure;
		private int _recordLimit = 0;

		protected override string ConnectionKey { get { return "sys"; } }

		public Reader(string commandText)
			: base(commandText)
		{

		}

		public Reader(string commandText, CommandType type)
			: base(commandText)
		{
			_commandType = type;
		}

		public Reader(string commandText, IDataTransaction transaction)
			: base(commandText)
		{
			SetConnection(transaction);
		}

		public Reader(string commandText, CommandType commandType, IDataTransaction transaction)
			: base(commandText)
		{
			_commandType = commandType;
			SetConnection(transaction);
		}

		public virtual S ExecuteScalar<S>(S defaultValue)
		{
			SqlCommand command = createCommand();

			try
			{
				prepareConnection();

				object res = command.ExecuteScalar();

				if (res == null || res == DBNull.Value)
					return defaultValue;
				else
					return (S)Convert.ChangeType(res, typeof(S));
			}
			finally
			{
				finalize(null);
			}
		}

		public virtual T SelectWithId(int id)
		{
			CreateParameter("@id", id);

			return ExecuteSingleRow();
		}

		public virtual T ExecuteSingleRow()
		{
			SqlCommand command = createCommand();
			SqlDataReader rdr = null;

			try
			{
				prepareConnection();
				rdr = command.ExecuteReader(CommandBehavior.SingleRow);

				if (rdr.Read())
				{
					DatabaseRecord d = new T();

					d.Create(rdr);

					Result.Add((T)Convert.ChangeType(d, typeof(T)));

					return Result[0];
				}

				return default(T);
			}
			finally
			{
				finalize(rdr);
			}
		}

		public virtual List<T> Execute()
		{
			SqlCommand command = createCommand();
			SqlDataReader rdr = null;

			try
			{
				//		RequestDiagnostics.NotifyRead();

				prepareConnection();
				rdr = command.ExecuteReader();

				int counter = 0;

				while (rdr.Read())
				{
					DatabaseRecord d = new T();

					d.Create(rdr);

					Result.Add((T)Convert.ChangeType(d, typeof(T)));

					counter++;

					if (RecordLimit > 0 && counter >= RecordLimit)
						break;
				}

				return Result;
			}
			finally
			{
				finalize(rdr);
			}
		}

		public virtual bool Execute(DatabaseRecord instance)
		{
			SqlCommand command = createCommand();
			SqlDataReader rdr = null;

			try
			{
				prepareConnection();
				rdr = command.ExecuteReader(CommandBehavior.SingleRow);

				if (rdr.Read())
				{
					instance.Create(rdr);

					return true;
				}
				else
					return false;
			}
			finally
			{
				finalize(rdr);
			}
		}

		public List<T> Result
		{
			get
			{
				if (_result == null)
					_result = new List<T>();

				return _result;
			}
		}

		public T Scalar
		{
			get
			{
				if (Result.Count == 0)
					return default(T);
				else
					return Result[0];
			}
		}

		public int RecordLimit
		{
			get { return _recordLimit; }
			set { _recordLimit = value; }
		}

		private SqlCommand createCommand()
		{
			SqlCommand command = null;

			command = Connection.CreateCommand();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			command.CommandText = CommandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

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

			if (CurrentTransaction == null && Connection.State != System.Data.ConnectionState.Closed)
				Connection.Close();
		}

		private void prepareConnection()
		{
			if (CurrentTransaction == null || Connection.State == ConnectionState.Closed)
				Connection.Open();
		}
	}
}