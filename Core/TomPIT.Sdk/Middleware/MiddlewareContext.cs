using System;
using System.Text.Json.Serialization;
using TomPIT.Connectivity;
using TomPIT.Data;
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
		private MiddlewareConnectionPool _connections = null;
		private IMiddlewareTransaction _transaction = null;

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
		internal IMiddlewareTransaction Transaction
		{
			get
			{
				if (_transaction != null)
					return _transaction;

				return BeginTransaction();
			}
			set
			{
				if (_transaction != null)
					throw new RuntimeException(SR.ErrTransactionNotNull);

				_transaction = value;
			}
		}

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

		internal IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)]string connection)
		{
			return Connections.OpenConnection(this, connection);
		}

		//public IDataReader<T> OpenReader<T>(IDataConnection connection, [CIP(CIP.CommandTextProvider)]string commandText)
		//{
		//	return new DataReader<T>(this)
		//	{
		//		Connection = connection,
		//		CommandText = commandText
		//	};
		//}

		//public IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)]string commandText)
		//{
		//	return new DataWriter(this)
		//	{
		//		Connection = connection,
		//		CommandText = commandText
		//	};
		//}

		public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText)
		{
			return new DataWriter(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText
			};
		}

		internal MiddlewareContext Owner { get; set; }

		private IMiddlewareTransaction BeginTransaction()
		{
			if (_transaction != null)
				return _transaction;

			if (Owner != null)
				return Owner.BeginTransaction();

			_transaction = new MiddlewareTransaction(this)
			{
				Id = Guid.NewGuid()
			};

			return _transaction;
		}

		internal void CloseConnections()
		{
			Connections.CloseConnections();
		}

		internal MiddlewareConnectionPool Connections
		{
			get
			{
				if (Owner != null)
					return Owner.Connections;

				if (_connections == null)
					_connections = new MiddlewareConnectionPool();

				return _connections;
			}
		}
	}
}
