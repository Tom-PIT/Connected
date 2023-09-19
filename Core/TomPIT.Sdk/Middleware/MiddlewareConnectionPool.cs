using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.Storage;
using TomPIT.Exceptions;
using TomPIT.Serialization;

namespace TomPIT.Middleware
{
	internal class MiddlewareConnectionPool : List<DataConnectionDescriptor>, IDisposable
	{
		public MiddlewareConnectionPool(IMiddlewareContext context, ITransactionContext transactions)
		{
			Context = context;
			Transactions = transactions;

			Transactions.StateChanged += OnTransactionStateChanged;
		}

		private IMiddlewareContext Context { get; }
		private ITransactionContext Transactions { get; }
		private int Identity { get; set; }
		public List<IDataConnection> DataConnections => this.OrderByDescending(f => f.Id).Select(f => f.Connection).ToList();

		public IDataConnection OpenConnection(string connection, ConnectionBehavior behavior, object arguments)
		{
			if (Transactions.State == MiddlewareTransactionState.Completed)
				behavior = ConnectionBehavior.Isolated;

			var descriptor = ComponentDescriptor.Connection(Context, connection);

			descriptor.Validate();

			var connectionConfiguration = descriptor.Configuration;
			var connectionString = connectionConfiguration.ResolveConnectionString(Context, ConnectionStringContext.User, arguments);

			var existing = behavior == ConnectionBehavior.Shared
				 ? TryExisting(connectionString, arguments)
				 : null;

			if (existing != null)
				return existing.Connection;

			var dataProvider = CreateDataProvider(Context, connectionConfiguration, connectionString.DataProvider);
			var con = dataProvider.OpenConnection(Context, connectionString.Value, behavior);

			if (behavior == ConnectionBehavior.Shared)
				AddConnection(dataProvider, connectionString.Value, arguments, con);

			return con;
		}

		public void CloseConnections()
		{
			foreach (var connection in this)
				connection.Connection.Close();
		}
		private void AddConnection(IDataProvider provider, string connectionString, object arguments, IDataConnection connection)
		{
			lock (this)
			{
				Add(new DataConnectionDescriptor
				{
					Connection = connection,
					ConnectionString = connectionString,
					DataProvider = provider,
					Arguments = arguments == null ? string.Empty : Serializer.Serialize(arguments),
					Id = Identity++
				});
			}
		}

		private DataConnectionDescriptor TryExisting(IConnectionString connectionString, object arguments)
		{
			var args = arguments == null ? string.Empty : Serializer.Serialize(arguments);

			lock (this)
			{
				return this.FirstOrDefault(f => f.DataProvider.Id == connectionString.DataProvider
					 && string.Compare(f.ConnectionString, connectionString.Value, true) == 0
					 && string.Compare(f.Arguments, args, true) == 0);
			}
		}

		/// <summary>
		/// This method creates data provider for the valid connection.
		/// </summary>
		/// <param name="connection">A connection instance which holds the information about its data provider</param>
		/// <returns>IDataProvider instance is a valid one is found.</returns>
		protected IDataProvider CreateDataProvider(IMiddlewareContext context, IConnectionConfiguration connection, Guid dataProvider)
		{
			/*
		 * Connection is not properly configured. We just notify the user about the issue.
		 */
			if (dataProvider == Guid.Empty)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection,
				};
			}

			var provider = context.Tenant.GetService<IDataProviderService>().Select(dataProvider);
			/*
		 * Connection has invalid data provider set. This can be for various reasons:
		 * --------------------------------------------------------------------------
		 * - data provider has been removed from the configuration
		 * - package misbehavior
		 */
			if (provider == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection
				};
			}

			return provider;
		}
		private void OnTransactionStateChanged(object? sender, EventArgs e)
		{
			if (Transactions.State == MiddlewareTransactionState.Committing)
				Commit();
			else if (Transactions.State == MiddlewareTransactionState.Reverting)
				Rollback();
		}

		private void Commit()
		{
			foreach (var connection in DataConnections)
				connection.Commit();

			CloseConnections();
		}

		private void Rollback()
		{
			foreach (var connection in DataConnections)
				connection.Rollback();

			CloseConnections();
		}

		public void Dispose()
		{
			foreach (var connection in DataConnections)
				connection.Dispose();

			Clear();
		}
	}
}
