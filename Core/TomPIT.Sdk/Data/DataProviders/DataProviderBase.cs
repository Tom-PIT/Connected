using System;
using System.Collections.Concurrent;
using System.Data;
using Newtonsoft.Json.Linq;

namespace TomPIT.Data.DataProviders
{
	public abstract class DataProviderBase<T> : IDataProvider where T : class, IDataConnection
	{
		private object _sync = new object();
		private ConcurrentDictionary<IDataCommandDescriptor, IDbCommand> _commands = null;

		public Guid Id { get; }
		public string Name { get; }

		protected DataProviderBase(string name, Guid id)
		{
			Id = id;
			Name = name;
		}

		public abstract IDataConnection OpenConnection(string connectionString, ConnectionBehavior behavior);

		private ConcurrentDictionary<IDataCommandDescriptor, IDbCommand> Commands
		{
			get
			{
				if (_commands == null)
				{
					lock (_sync)
					{
						if (_commands == null)
							_commands = new ConcurrentDictionary<IDataCommandDescriptor, IDbCommand>();
					}
				}

				return _commands;
			}
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

		public virtual int Execute(IDataCommandDescriptor command, IDataConnection connection)
		{
			EnsureOpen(connection);

			var com = ResolveCommand(command, connection);

			SetupParameters(command, com);

			foreach (var i in command.Parameters)
				SetParameterValue(com, i.Name, i.Value);

			var recordsAffected = Execute(command, com);

			foreach (var i in command.Parameters)
			{
				if (i.Direction == ParameterDirection.ReturnValue)
					i.Value = GetParameterValue(com, i.Name);
			}

			return recordsAffected;
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

		protected virtual int Execute(IDataCommandDescriptor command, IDbCommand cmd)
		{
			return cmd.ExecuteNonQuery();
		}

		public virtual JObject Query(IDataCommandDescriptor command, DataTable schema)
		{
			return Query(command, schema, null);
		}

		public virtual JObject Query(IDataCommandDescriptor command, DataTable schema, IDataConnection connection)
		{
			EnsureOpen(connection);

			var com = ResolveCommand(command, connection);

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
			var con = OpenConnection(connectionString, ConnectionBehavior.Isolated);

			con.Open();
			con.Close();
		}

		protected virtual IDbCommand ResolveCommand(IDataCommandDescriptor command, IDataConnection connection)
		{
			if (Commands.TryGetValue(command, out IDbCommand existing))
				return existing;

			lock (_sync)
			{
				if (Commands.TryGetValue(command, out IDbCommand existing2))
					return existing2;

				var r = connection.CreateCommand();

				r.CommandText = command.CommandText;
				r.CommandType = command.CommandType;
				r.CommandTimeout = command.CommandTimeout;

				if (connection.Transaction != null)
					r.Transaction = connection.Transaction;

				Commands.TryAdd(command, r);

				return r;
			}
		}

		private void EnsureOpen(IDataConnection connection)
		{
			if (connection == null)
				return;

			if (connection.State == ConnectionState.Open)
				return;

			connection.Open();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var command in Commands)
					command.Value.Dispose();

				Commands.Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
