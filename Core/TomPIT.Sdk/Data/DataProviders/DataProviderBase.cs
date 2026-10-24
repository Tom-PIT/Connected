using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using TomPIT.Middleware;

namespace TomPIT.Data.DataProviders
{
	public abstract class DataProviderBase<T> : IDataProvider where T : class, IDataConnection
	{
		private SemaphoreSlim _semaphore;

		public Guid Id { get; }
		public string Name { get; }

		protected DataProviderBase(string name, Guid id)
		{
			_semaphore = new(1);

			Id = id;
			Name = name;
		}

		public abstract IDataConnection OpenConnection(IMiddlewareContext context, string connectionString, ConnectionBehavior behavior);

		public virtual int Execute(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection)
		{
			EnsureOpen(connection);

			using var com = ResolveCommand(command, connection);

			SetupParameters(command, com);

			foreach (var i in command.Parameters)
				SetParameterValue(connection, com, i.Name, i.Value);

			var recordsAffected = Execute(command, com, (context as MiddlewareContext)?.CancellationToken);

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

		protected virtual object? GetParameterValue(IDbCommand command, string parameterName)
		{
			return null;
		}

		protected virtual void SetupParameters(IDataCommandDescriptor command, IDbCommand cmd)
		{
		}

		protected virtual int Execute(IDataCommandDescriptor command, IDbCommand cmd, CancellationToken? cancellationToken = null)
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

			using var com = ResolveCommand(command, connection);

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
				if (rdr is not null && !rdr.IsClosed)
					CloseReader(rdr);
			}
		}

		protected virtual void CloseReader(IDataReader reader)
		{
			reader.Close();
		}

		public virtual R? Select<R>(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection)
		{
			EnsureOpen(connection);

			using var com = ResolveCommand(command, connection);

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
				if (rdr is not null && !rdr.IsClosed)
					CloseReader(rdr);
			}
		}

		public virtual void TestConnection(IMiddlewareContext context, string connectionString)
		{
			var con = OpenConnection(context, connectionString, ConnectionBehavior.Isolated);

			con.Open();
			con.Close();
		}

		/*
		 * Commands are intentionally not cached. A command is bound to the connection and transaction it was
		 * created with, while the provider is shared across connections, so a provider level cache can never
		 * safely reuse a command. The previous cache was keyed by descriptor instance which is created per call,
		 * so it practically never hit and grew without bound, retaining every command with its parameters and
		 * result metadata (a memory leak of tens of thousands of commands for a few hundred distinct statements).
		 *
		 * Performance: creating a command and its parameters costs a few microseconds and a small short lived
		 * allocation, which is negligible compared to the database round trip. Server side plan caching does not
		 * depend on reusing the client command object, and commands are not prepared, so nothing is lost by
		 * creating them per call.
		 */
		protected virtual IDbCommand ResolveCommand(IDataCommandDescriptor command, IDataConnection connection)
		{
			var r = connection.CreateCommand();

			r.CommandText = command.CommandText;
			r.CommandType = command.CommandType;
			r.CommandTimeout = command.CommandTimeout;

			if (connection.Transaction != null)
				r.Transaction = connection.Transaction;

			return r;
		}

		private void EnsureOpen(IDataConnection connection)
		{
			if (connection is null)
				return;

			if (connection.State == ConnectionState.Open)
				return;

			_semaphore.WaitAsync();

			try
			{
				if (connection.State == ConnectionState.Open)
					return;

				connection.Open();
			}
			finally
			{
				_semaphore.Release();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				_semaphore?.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
