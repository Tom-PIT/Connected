using System;
using System.Text.Json.Serialization;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using TomPIT.Reflection;
using TomPIT.Security;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public class MiddlewareContext : IMiddlewareContext, IElevationContext
	{
		#region Members
		private ElevationContextState _elevationState = ElevationContextState.Revoked;
		private IMiddlewareServices _services = null;
		private ITenant _tenant = null;
		private IMiddlewareInterop _interop = null;
		private IMiddlewareEnvironment _environment = null;
		private MiddlewareConnectionPool _connections = null;
		private IMiddlewareTransaction _transaction = null;
		#endregion

		#region Constructors

		public MiddlewareContext()
		{
			Initialize(null);
		}

		public MiddlewareContext(IMiddlewareObject owner) : this(owner?.Context)
		{
		}

		public MiddlewareContext(IMiddlewareContext sender)
		{
			var endpoint = sender is ITenantProvider c ? c.Endpoint : null;

			Initialize(endpoint);

			if (sender is MiddlewareContext mw)
			{
				((MiddlewareDiagnosticService)Services.Diagnostic).MetricParent = ((MiddlewareDiagnosticService)mw.Services.Diagnostic).MetricParent;

				Owner = mw;
			}
		}

		public MiddlewareContext(string endpoint)
		{
			Initialize(endpoint);
		}

		#endregion

		#region Properties
		[JsonIgnore]
		ElevationContextState IElevationContext.State => Owner == null ? _elevationState : Owner._elevationState;
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


		internal MiddlewareContext Owner { get; set; }

		[JsonIgnore]
		public string Endpoint { get; protected set; }

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

		#endregion

		#region Methods

		public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
		{
			if (Transaction.State == MiddlewareTransactionState.Active)
				return OpenReader<T>(connection, commandText, ConnectionBehavior.Shared);
			else
				return OpenReader<T>(connection, commandText, ConnectionBehavior.Isolated);
		}

		public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
		{
			return new DataReader<T>(this)
			{
				Connection = OpenConnection(connection, behavior),
				CommandText = commandText
			};
		}

		public IDataReader<T> OpenReader<T>(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
		{
			if (Transaction.State == MiddlewareTransactionState.Active)
				return OpenWriter(connection, commandText, ConnectionBehavior.Shared);
			else
				return OpenWriter(connection, commandText, ConnectionBehavior.Isolated);
		}

		public IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText)
		{
			return new DataWriter(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
		{
			return new DataWriter(this)
			{
				Connection = OpenConnection(connection, behavior),
				CommandText = commandText
			};
		}

		#endregion

		protected void Initialize(string endpoint)
		{
			Endpoint = endpoint;

			if (string.IsNullOrWhiteSpace(endpoint))
				Endpoint = Tenant?.Url;
		}

		internal IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior)
		{
			return OpenConnection(connection, behavior, null);
		}

		public IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior, object arguments)
		{
			try
			{
				return Connections.OpenConnection(this, connection, behavior, arguments);
			}
			catch
			{
				throw;
			}
		}

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

		void IElevationContext.Grant()
		{
			_elevationState = ElevationContextState.Granted;
		}

		void IElevationContext.Revoke()
		{
			_elevationState = ElevationContextState.Granted;
		}

		public T OpenModel<T>() where T : IModelComponent
		{
			var result = TypeExtensions.CreateInstance<IModelComponent>(typeof(T));

			ReflectionExtensions.SetPropertyValue(result, nameof(result.Context), this);

			return (T)result;
		}
	}
}
