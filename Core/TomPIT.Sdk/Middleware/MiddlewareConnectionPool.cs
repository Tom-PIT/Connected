using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Exceptions;

namespace TomPIT.Middleware
{
	internal class MiddlewareConnectionPool : ConcurrentDictionary<MiddlewareContext, List<DataConnectionDescriptor>>, IDisposable
	{
		private int Identity { get; set; }
		public IDataConnection OpenConnection(MiddlewareContext sender, string connection, ConnectionBehavior behavior)
		{
			var descriptor = ComponentDescriptor.Connection(sender, connection);

			descriptor.Validate();

			var existing = behavior == ConnectionBehavior.Shared
				? TryExisting(sender, connection, descriptor.Configuration)
				: null;

			if (existing != null)
				return existing.Connection;

			if (sender.Owner != null)
				return sender.Owner.OpenConnection(connection, behavior);

			var dataProvider = CreateDataProvider(sender, descriptor.Configuration);
			var con = dataProvider.OpenConnection(descriptor.Configuration.Value, behavior);

			if (behavior == ConnectionBehavior.Shared)
				AddConnection(sender, dataProvider, descriptor.Configuration.Value, con);

			return con;
		}

		public void CloseConnections()
		{
			foreach (var context in this)
			{
				foreach (var connection in context.Value)
					connection.Connection.Close();
			}
		}
		private void AddConnection(MiddlewareContext context, IDataProvider provider, string connectionString, IDataConnection connection)
		{
			List<DataConnectionDescriptor> items = null;

			if (ContainsKey(context))
				items = this[context];
			else
			{
				items = new List<DataConnectionDescriptor>();
				TryAdd(context, items);
			}

			items.Add(new DataConnectionDescriptor
			{
				Connection = connection,
				ConnectionString = connectionString,
				DataProvider = provider,
				Id = Identity++
			});
		}

		private DataConnectionDescriptor TryExisting(MiddlewareContext context, string connection, IConnectionConfiguration configuration)
		{
			if (!ContainsKey(context))
				return null;

			return this[context].FirstOrDefault(f => f.DataProvider.Id == configuration.DataProvider && string.Compare(f.ConnectionString, configuration.Value, true) == 0);
		}

		/// <summary>
		/// This method creates data provider for the valid connection.
		/// </summary>
		/// <param name="connection">A connection instance which holds the information about its data provider</param>
		/// <returns>IDataProvider instance is a valid one is found.</returns>
		protected IDataProvider CreateDataProvider(IMiddlewareContext context, IConnectionConfiguration connection)
		{
			/*
			 * Connection is not properly configured. We just notify the user about the issue.
			 */
			if (connection.DataProvider == Guid.Empty)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection,
				};
			}

			var provider = context.Tenant.GetService<IDataProviderService>().Select(connection.DataProvider);
			/*
			 * Connection has invalid data provider set. This can be for various reasons:
			 * --------------------------------------------------------------------------
			 * - data provider has been removed from the configuration
			 * - package misbehavior
			 */
			if (provider == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, provider.Name))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection
				};
			}

			return provider;
		}

		public void Dispose()
		{
			foreach (var context in this)
			{
				foreach (var connection in context.Value)
					connection.Connection.Dispose();
			}

			Clear();
		}

		public List<IDataConnection> DataConnections
		{
			get
			{
				var result = new List<DataConnectionDescriptor>();

				foreach (var context in this)
				{
					foreach (var connection in context.Value)
						result.Add(connection);
				}

				return result.OrderByDescending(f => f.Id).Select(f => f.Connection).ToList();
			}
		}
	}
}
