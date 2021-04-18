using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using TomPIT.Middleware;

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

		public abstract IDataConnection OpenConnection(IMiddlewareContext context, string connectionString, ConnectionBehavior behavior);

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

		public virtual int Execute(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection)
		{
			EnsureOpen(connection);

			var com = ResolveCommand(command, connection);

			SetupParameters(command, com);

			foreach (var i in command.Parameters)
				SetParameterValue(connection, com, i.Name, i.Value);

			var recordsAffected = Execute(command, com);

			foreach (var i in command.Parameters)
			{
				if (i.Direction == ParameterDirection.ReturnValue)
					i.Value = GetParameterValue(com, i.Name);
			}

			return recordsAffected;
		}

		protected virtual void SetParameterValue(IDataConnection connection, IDbCommand command, string parameterName, object value)
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

		public virtual List<R> Query<R>(IMiddlewareContext context, IDataCommandDescriptor command)
		{
			return Query<R>(context, command, null);
		}

		public virtual R Select<R>(IMiddlewareContext context, IDataCommandDescriptor command)
		{
			return Select<R>(context, command, null);
		}

		public virtual List<R> Query<R>(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection)
		{
			EnsureOpen(connection);

			var com = ResolveCommand(command, connection);

			IDataReader rdr = null;

			try
			{
				SetupParameters(command, com);

				foreach (var i in command.Parameters)
					SetParameterValue(connection, com, i.Name, i.Value);

				rdr = com.ExecuteReader();
				var result = new List<R>();
				var mappings = new FieldMappings<R>(context, rdr);

				while (rdr.Read())
					result.Add(mappings.CreateInstance(rdr));

				return result;
			}
			finally
			{
				if (rdr != null && !rdr.IsClosed)
					rdr.Close();
			}
		}

		public virtual R Select<R>(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection)
		{
			EnsureOpen(connection);

			var com = ResolveCommand(command, connection);

			IDataReader rdr = null;

			try
			{
				SetupParameters(command, com);

				foreach (var i in command.Parameters)
					SetParameterValue(connection, com, i.Name, i.Value);

				rdr = com.ExecuteReader(CommandBehavior.SingleRow);
				var mappings = new FieldMappings<R>(context, rdr);

				if (rdr.Read())
					return mappings.CreateInstance(rdr);

				return default;
			}
			finally
			{
				if (rdr != null && !rdr.IsClosed)
					rdr.Close();
			}
		}

		public virtual void TestConnection(IMiddlewareContext context, string connectionString)
		{
			var con = OpenConnection(context, connectionString, ConnectionBehavior.Isolated);

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

		private static void EnsureOpen(IDataConnection connection)
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
