using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Exceptions;
using TomPIT.Serialization;

namespace TomPIT.Middleware
{
	internal class MiddlewareConnectionPool : ConcurrentDictionary<MiddlewareContext, List<DataConnectionDescriptor>>, IDisposable
	{
		private int Identity { get; set; }
		public IDataConnection OpenConnection(MiddlewareContext sender, string connection, ConnectionBehavior behavior, object arguments)
		{
			var descriptor = ComponentDescriptor.Connection(sender, connection);

			descriptor.Validate();

			var connectionConfiguration = descriptor.Configuration;
			var connectionString = connectionConfiguration.ResolveConnectionString(sender, ConnectionStringContext.User, arguments);

			var existing = behavior == ConnectionBehavior.Shared
				? TryExisting(sender, connectionString, arguments)
				: null;

			if (existing != null)
				return existing.Connection;

			if (sender.Owner != null)
				return sender.Owner.OpenConnection(connection, behavior, arguments);

			var dataProvider = CreateDataProvider(sender, connectionConfiguration, connectionString.DataProvider);
			var con = dataProvider.OpenConnection(connectionString.Value, behavior);

			if (behavior == ConnectionBehavior.Shared)
				AddConnection(sender, dataProvider, connectionString.Value, arguments, con);

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
		private void AddConnection(MiddlewareContext context, IDataProvider provider, string connectionString, object arguments, IDataConnection connection)
		{
			List<DataConnectionDescriptor> items = null;

			if (ContainsKey(context))
				items = this[context];
			else
			{
				items = new List<DataConnectionDescriptor>();
				TryAdd(context, items);
			}

			lock (items)
			{
				items.Add(new DataConnectionDescriptor
				{
					Connection = connection,
					ConnectionString = connectionString,
					DataProvider = provider,
					Arguments = arguments == null ? string.Empty : Serializer.Serialize(arguments),
					Id = Identity++
				});
			}
		}

		private DataConnectionDescriptor TryExisting(MiddlewareContext context, IConnectionString connectionString, object arguments)
		{
			if (!ContainsKey(context))
				return null;

			var args = arguments == null ? string.Empty : Serializer.Serialize(arguments);

			var items = this[context];

			lock (items)
			{
				return items.FirstOrDefault(f => f.DataProvider.Id == connectionString.DataProvider
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
