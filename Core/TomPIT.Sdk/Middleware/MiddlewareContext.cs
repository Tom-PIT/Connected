using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public class MiddlewareContext : IMiddlewareContext
	{
		private IMiddlewareServices _services = null;
		private ITenant _tenant = null;
		private IMiddlewareInterop _interop = null;
		private IMiddlewareEnvironment _environment = null;
		private List<IDataConnection> _connections = null;
		private IDataConnection _connection = null;

		public MiddlewareContext()
		{
			Initialize(null);
		}

		public MiddlewareContext(IMiddlewareContext sender)
		{
			var endpoint = sender is ITenantProvider c ? c.Endpoint : null;

			Initialize(endpoint);

			if (sender is MiddlewareContext mw)
				((MiddlewareDiagnosticService)Services.Diagnostic).MetricParent = ((MiddlewareDiagnosticService)mw.Services.Diagnostic).MetricParent;
		}

		public MiddlewareContext(string endpoint)
		{
			Initialize(endpoint);
		}

		protected void Initialize(string endpoint)
		{
			Endpoint = endpoint;

			if (string.IsNullOrWhiteSpace(endpoint))
				Endpoint = Tenant?.Url;
		}

		[JsonIgnore]
		public virtual IMiddlewareServices Services
		{
			get
			{
				if (_services == null)
					_services = new MiddlewareServices(this);

				return _services;
			}
		}
		[JsonIgnore]
		public string Endpoint { get; protected set; }
		internal IMiddlewareTransaction Transaction { get; set; }

		[JsonIgnore]
		public IMiddlewareInterop Interop
		{
			get
			{
				if (_interop == null)
					_interop = new MiddlewareInterop(this);

				return _interop;
			}
		}

		[JsonIgnore]
		public IMiddlewareEnvironment Environment
		{
			get
			{
				if (_environment == null)
					_environment = new MiddlewareEnvironment();

				return _environment;
			}
		}

		[JsonIgnore]
		public ITenant Tenant
		{
			get
			{
				if (_tenant == null)
				{
					if (string.IsNullOrWhiteSpace(Endpoint))
						_tenant = MiddlewareDescriptor.Current.Tenant;
					else
						_tenant = Shell.GetService<IConnectivityService>().SelectTenant(Endpoint);
				}

				return _tenant;
			}
		}

		public IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)]string connection)
		{
			if (Connection == null && Owner != null)
				return Owner.OpenConnection(connection);

			var descriptor = ComponentDescriptor.Connection(this, connection);

			descriptor.Validate();

			var dataProvider = CreateDataProvider(descriptor.Configuration);

			var con = dataProvider.OpenConnection(descriptor.Configuration.Value, Connection, Transaction);

			Connections.Add(con);
			Connection = con;

			return con;
		}

		public IDataReader<T> OpenReader<T>(IDataConnection connection, [CIP(CIP.CommandTextProvider)]string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)]string commandText)
		{
			return new DataWriter(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText,
				CloseConnection = true
			};
		}

		public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText)
		{
			return new DataWriter(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText,
				CloseConnection = true
			};
		}

		/// <summary>
		/// This method creates data provider for the valid connection.
		/// </summary>
		/// <param name="connection">A connection instance which holds the information about its data provider</param>
		/// <returns>IDataProvider instance is a valid one is found.</returns>
		protected IDataProvider CreateDataProvider(IConnectionConfiguration connection)
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

			var provider = Tenant.GetService<IDataProviderService>().Select(connection.DataProvider);
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

		internal IDataConnection Connection
		{
			get
			{
				if (_connection != null)
					return _connection;

				_connection = Owner?.Connection;

				return _connection;
			}
			set { _connection = value; }
		}
		internal MiddlewareContext Owner { get; set; }
		internal void CloseConnections()
		{
			foreach (var connection in Connections)
				connection.Close();
		}

		private bool _disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					foreach (var connection in Connections)
						connection.Dispose();

					Connections.Clear();
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private List<IDataConnection> Connections
		{
			get
			{
				if (_connections == null)
					_connections = new List<IDataConnection>();

				return _connections;
			}
		}
	}
}
