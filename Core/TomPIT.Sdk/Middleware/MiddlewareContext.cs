using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.Storage;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using TomPIT.Middleware.Storage;
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
		private object _authorizationOwner;
		private CancellationTokenSource _cancellationTokenSource = new();
		private readonly object _sync = new();
		private IServiceScope? _scope;
		#endregion

		#region Constructors
		public MiddlewareContext()
		{
			Transactions = new TransactionContext(this);
			Interop = new MiddlewareInterop(this);
			Services = new MiddlewareServices(this, Transactions);
			Environment = new MiddlewareEnvironment();
			//Connections = new ConnectionProvider(this, Transactions);
			ModelConnections = new(this, Transactions);
			Tenant = MiddlewareDescriptor.Current.Tenant;

			_authorizationOwner = this;

			Initialize();
		}

		public MiddlewareContext(bool interactive) : this()
		{
			((MiddlewareEnvironment)Environment).IsInteractive = interactive;
		}

		[Obsolete("Remains to keep code compatibility. Use the parameterless constructor instead.")]
		public MiddlewareContext(string endpoint = null) : this()
		{
		}
		#endregion

		#region Properties
		private bool Disposed { get; set; }

		private bool IsReadOnly => ((IIOBehaviorContext)this).Behavior == EnvironmentIOBehavior.ReadOnly;
		internal ITransactionContext Transactions { get; }
		//private IConnectionProvider Connections { get; }
		private MiddlewareConnectionPool ModelConnections { get; }
		[JsonIgnore]
		ElevationContextState IElevationContext.State { get; set; } = ElevationContextState.Revoked;

		[JsonIgnore]
		object IElevationContext.AuthorizationOwner { get { return _authorizationOwner; } set { _authorizationOwner = value; } }

		[JsonIgnore]
		public virtual IMiddlewareServices Services { get; }

		[JsonIgnore]
		public IMiddlewareEnvironment Environment { get; }

		[JsonIgnore]
		public ITenant Tenant { get; }

		[JsonIgnore]
		public IMiddlewareInterop Interop { get; }

		[JsonIgnore]
		public CancellationToken CancellationToken => _cancellationTokenSource.Token;

		EnvironmentIOBehavior IIOBehaviorContext.Behavior { get; set; }

		#endregion

		#region Methods

		public TService? GetService<TService>()
		{
			if (Disposed)
				return default;

			if (_scope is null)
			{
				lock (_sync)
				{
					var host = Tenant.GetService<IRuntimeService>().Host;

					if (host is null)
						return default;

					_scope ??= host.ApplicationServices.CreateScope();
				}
			}

			if (_scope.ServiceProvider is null)
				return default;

			return _scope.ServiceProvider.GetService<TService>();
		}

		public void Cancel()
		{
			_cancellationTokenSource.Cancel();
		}

		public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
		{
			if (Transactions.State == MiddlewareTransactionState.Active)
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

			if (Transactions.State == MiddlewareTransactionState.Active)
				return OpenWriter(connection, commandText, ConnectionBehavior.Shared);
			else
				return OpenWriter(connection, commandText, ConnectionBehavior.Isolated);
		}

		public IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText)
		{
			if (IsReadOnly)
				throw new TomPITException(SR.ErrContextInReadonlyMode);

			return new DataWriter(this, Transactions)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
		{
			if (IsReadOnly)
				throw new TomPITException(SR.ErrContextInReadonlyMode);

			return new DataWriter(this, Transactions)
			{
				Connection = OpenConnection(connection, behavior),
				CommandText = commandText
			};
		}

		public void MarkUnstable()
		{
			_cancellationTokenSource.Cancel(true);
		}
		#endregion

		protected void Initialize()
		{
			((IIOBehaviorContext)this).Behavior = Tenant.GetService<IRuntimeService>().IOBehavior;
		}

		public IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior)
		{
			return OpenConnection(connection, behavior, null);
		}
		public IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior, object arguments)
		{
			try
			{
				CancellationToken.ThrowIfCancellationRequested();

				return ModelConnections.OpenConnection(connection, behavior, arguments);
			}
			catch (OperationCanceledException ex)
			{
				//TODO localize
				throw new Exception("Cannot open new connection on unstable context", ex);
			}
			catch
			{
				throw;
			}
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
					if (_scope is not null)
					{
						_scope.Dispose();
						_scope = null;
					}

					//Connections.Dispose();
					ModelConnections.Dispose();
					Interop.Dispose();
					Services.Dispose();
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
