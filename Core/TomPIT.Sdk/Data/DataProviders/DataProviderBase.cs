using System;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.Data.DataProviders
{
	public abstract class DataProviderBase<T> : IDataProvider where T : class, IDataConnection
	{
		public Guid Id { get; }
		public string Name { get; }

		protected DataProviderBase(string name, Guid id)
		{
			Id = id;
			Name = name;
		}

		public abstract IDataConnection OpenConnection(string connectionString, ConnectionBehavior behavior);
		protected virtual IDbConnection ResolveConnection(IDataCommandDescriptor command, IDataConnection connection)
		{
			T c = null;

			if (connection != null)
			{
				c = connection as T;

				if (c == null)
					throw new RuntimeException(string.Format(SR.ErrInvalidConnectionType, typeof(T).ShortName()));
			}

			if (c == null)
				return CreateConnection(command.ConnectionString);

			return c.Connection;
		}

		protected DbType ResolveType(ICommandParameter i)
		{
			if (i.DataType != null)
				return Types.ToDbType(Types.ToDataType(i.DataType));

			if (i.Value == null)
				return DbType.String;

			return Types.ToDbType(Types.ToDataType(i.Value.GetType()));
		}

		protected JObject CreateDataRow(IDataReader rdr)
		{
			var row = new JObject();

			for (var i = 0; i < rdr.FieldCount; i++)
				row.Add(rdr.GetName(i), new JValue(GetValue(rdr, i)));

			return row;
		}

		protected JObject CreateDataRow(IDataReader rdr, DataTable schema)
		{
			var row = new JObject();

			foreach (DataColumn i in schema.Columns)
			{
				if (i.ExtendedProperties.Contains("unbound"))
				{
					row.Add(i.ColumnName, string.Empty);

					continue;
				}

				var mapping = i.ColumnName;

				if (i.ExtendedProperties.Contains("mapping"))
					mapping = (string)i.ExtendedProperties["mapping"];

				int ord = rdr.GetOrdinal(mapping);
				var value = GetValue(rdr, ord);

				row.Add(i.ColumnName, new JValue(value == DBNull.Value ? null : value));
			}

			return row;
		}

		protected object GetValue(IDataReader reader, int index)
		{
			var value = reader.GetValue(index);

			if (value == DBNull.Value || value == null)
				return null;

			if (value is DateTime date)
				return DateTime.SpecifyKind(date, DateTimeKind.Utc);

			return value;
		}

		public virtual void Execute(IDataCommandDescriptor command)
		{
			Execute(command, null);
		}

		public virtual void Execute(IDataCommandDescriptor command, IDataConnection connection)
		{
			if (connection.Connection.State == ConnectionState.Closed)
				connection.Open();

			var con = ResolveConnection(command, connection);
			var com = ResolveCommand(command, con, connection);

			SetupParameters(command, com);

			foreach (var i in command.Parameters)
				SetParameterValue(com, i.Name, i.Value);

			Execute(command, con, com);

			foreach (var i in command.Parameters)
			{
				if (i.Direction == ParameterDirection.ReturnValue)
					i.Value = GetParameterValue(com, i.Name);
			}
		}

		protected virtual void SetParameterValue(IDbCommand command, string parameterName, object value)
		{

		}

		protected virtual object GetParameterValue(IDbCommand command, string parameterName)
		{
			return null;
		}

		protected virtual void SetupParameters(IDataCommandDescriptor command, IDbCommand cmd)
		{
		}

		protected virtual void Execute(IDataCommandDescriptor command, IDbConnection connection, IDbCommand cmd)
		{
			if (connection.State == ConnectionState.Closed)
				connection.Open();

			cmd.ExecuteNonQuery();
		}

		public virtual JObject Query(IDataCommandDescriptor command, DataTable schema)
		{
			return Query(command, schema, null);
		}

		public virtual JObject Query(IDataCommandDescriptor command, DataTable schema, IDataConnection connection)
		{
			var con = ResolveConnection(command, connection);
			var com = ResolveCommand(command, con, connection);

			if (con.State == ConnectionState.Closed)
				con.Open();

			IDataReader rdr = null;

			try
			{
				SetupParameters(command, com);

				foreach (var i in command.Parameters)
					SetParameterValue(com, i.Name, i.Value);

				rdr = com.ExecuteReader();
				var r = new JObject();
				var a = new JArray();

				r.Add("data", a);

				while (rdr.Read())
				{
					var row = schema == null
						? CreateDataRow(rdr)
						: CreateDataRow(rdr, schema);

					a.Add(row);
				}

				return r;
			}
			finally
			{
				if (rdr != null && !rdr.IsClosed)
					rdr.Close();
			}
		}

		public virtual void TestConnection(string connectionString)
		{
			var con = CreateConnection(connectionString);

			con.Open();
			con.Close();
		}

		protected abstract IDbConnection CreateConnection(string connectionString);

		protected virtual IDbCommand ResolveCommand(IDataCommandDescriptor command, IDbConnection connection, IDataConnection dataConnection)
		{
			T dc = default;

			if (dataConnection != null)
			{
				if (!(dataConnection is T))
					throw new RuntimeException(string.Format(SR.ErrInvalidConnectionType, typeof(T).ShortName()));

				dc = dataConnection as T;
			}

			var r = connection.CreateCommand();

			r.CommandText = command.CommandText;
			r.CommandType = command.CommandType;
			r.CommandTimeout = command.CommandTimeout;

			if (dc != default)
			{
				if (dc.Transaction != null)
					r.Transaction = dc.Transaction;
			}

			return r;
		}
	}
}
