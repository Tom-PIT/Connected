using System;
using System.Text.Json.Serialization;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Security;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
    public class MiddlewareContext : IMiddlewareContext, IElevationContext, IIOBehaviorContext, IDisposable
    {
        #region Members
        private ElevationContextState _elevationState = ElevationContextState.Revoked;
        private object _authorizationOwner = null;
        private IMiddlewareServices _services = null;
        private ITenant _tenant = null;
        private IMiddlewareInterop _interop = null;
        private IMiddlewareEnvironment _environment = null;
        private MiddlewareConnectionPool _connections = null;
        private IMiddlewareTransaction _transaction = null;
        private bool _isReadonly = false;
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
        private bool Disposed { get; set; }

        private bool IsReadOnly => ((IIOBehaviorContext)this).Behavior == EnvironmentIOBehavior.ReadOnly;

        [JsonIgnore]
        ElevationContextState IElevationContext.State
        {
            get
            {

                if (Owner == null)
                    return _elevationState;

                if (Owner is IElevationContext elevationContext)
                    return elevationContext.State;

                return _elevationState;
            }
            set
            {
                if (Owner == null || Owner is not IElevationContext elevationOwner)
                    _elevationState = value;
                else
                    elevationOwner.State = value;
            }
        }

        [JsonIgnore]
        object IElevationContext.AuthorizationOwner
        {
            get
            {
                if (Owner == null)
                    return _authorizationOwner;

                if (Owner is IElevationContext elevationContext)
                    return elevationContext.AuthorizationOwner;

                return _authorizationOwner;
            }
            set
            {
                if (Owner == null || Owner is not IElevationContext elevationOwner)
                    _authorizationOwner = value;
                else
                    elevationOwner.AuthorizationOwner = value;
            }
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

                if (_connections == null && !Disposed)
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

        EnvironmentIOBehavior IIOBehaviorContext.Behavior { get; set; }

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
            if (IsReadOnly)
                throw new TomPITException(SR.ErrContextInReadonlyMode);

            if (Transaction.State == MiddlewareTransactionState.Active)
                return OpenWriter(connection, commandText, ConnectionBehavior.Shared);
            else
                return OpenWriter(connection, commandText, ConnectionBehavior.Isolated);
        }

        public IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            if (IsReadOnly)
                throw new TomPITException(SR.ErrContextInReadonlyMode);

            return new DataWriter(this)
            {
                Connection = connection,
                CommandText = commandText
            };
        }

        public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
        {
            if (IsReadOnly)
                throw new TomPITException(SR.ErrContextInReadonlyMode);

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

            ((IIOBehaviorContext)this).Behavior = Tenant.GetService<IRuntimeService>().IOBehavior;
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
            if (_transaction is not null)
                return _transaction;

            if (Owner is not null)
                return Owner.BeginTransaction();

            _transaction = new MiddlewareTransaction(this);

            return _transaction;
        }

        internal void CloseConnections()
        {
            Connections.CloseConnections();
        }

        public T OpenModel<T>() where T : IModelComponent
        {
            var result = TypeExtensions.CreateInstance<IModelComponent>(typeof(T));

            ReflectionExtensions.SetPropertyValue(result, nameof(result.Context), this);

            return (T)result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (Owner == null)
                        Connections.Dispose();

                    if (_interop != null)
                    {
                        _interop.Dispose();
                        _interop = null;
                    }

                    if (_services is not null)
                    {
                        _services.Dispose();
                        _services = null;
                    }

                    _connections = null;
                    _authorizationOwner = null;
                }

                Disposed = true;
            }
        }

        ~MiddlewareContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
       
    }
}
