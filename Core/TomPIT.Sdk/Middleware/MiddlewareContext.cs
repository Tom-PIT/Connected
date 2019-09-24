using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Exceptions;
using TomPIT.Middleware.Interop;
using TomPIT.Middleware.Services;
using TomPIT.Serialization;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public class MiddlewareContext : IMiddlewareContext
	{
		private IMiddlewareServices _services = null;
		private ITenant _tenant = null;

		public MiddlewareContext()
		{

		}

		public MiddlewareContext(IMiddlewareContext sender) : this(sender, sender?.MicroService)
		{
		}

		public MiddlewareContext(IMiddlewareContext sender, IMicroService microService)
		{
			var endpoint = sender is ITenantProvider c ? c.Endpoint : null;

			Initialize(endpoint, microService);

			if (sender is MiddlewareContext mw)
				((MiddlewareDiagnosticService)Services.Diagnostic).MetricParent = ((MiddlewareDiagnosticService)mw.Services.Diagnostic).MetricParent;
		}

		public MiddlewareContext(string endpoint)
		{
			Initialize(endpoint, null);
		}

		public MiddlewareContext(string endpoint, IMicroService microService)
		{
			Initialize(endpoint, microService);
		}

		public MiddlewareContext(IMicroService microService)
		{
			Initialize(null, microService);
		}

		protected void Initialize(string endpoint, IMicroService microService)
		{
			Endpoint = endpoint;
			MicroService = microService;

			if (string.IsNullOrWhiteSpace(endpoint))
				Endpoint = MiddlewareDescriptor.Current.Tenant?.Url;
		}

		public virtual IMiddlewareServices Services
		{
			get
			{
				if (_services == null)
					_services = new MiddlewareServices(this);

				return _services;
			}
		}

		public virtual IMicroService MicroService { get; protected set; }

		public string Endpoint { get; protected set; }

		public ITenant Tenant
		{
			get
			{
				if (_tenant == null)
				{
					if (string.IsNullOrWhiteSpace(Endpoint))
					{
						_tenant = MiddlewareDescriptor.Current.Tenant;

						if (_tenant == null)
							throw new TomPITException(SR.ErrCannotResolveTenant);
					}
					else
						_tenant = Shell.GetService<IConnectivityService>().SelectTenant(Endpoint);
				}

				return _tenant;
			}
		}


		public R Invoke<R>([CAP(CAP.ApiProvider)]string api)
		{
			var descriptor = ComponentDescriptor.Api(this, api);
			var invoker = new ApiInvoker(this);
			var result = invoker.Invoke(this as IApiExecutionScope, descriptor, EventArgs.Empty);

			return Marshall.Convert<R>(result);
		}

		public R Invoke<R, A>([CAP(CAP.ApiProvider)] string api, A e)
		{
			var descriptor = ComponentDescriptor.Api(this, api);
			var invoker = new ApiInvoker(this);
			var result = invoker.Invoke(this as IApiExecutionScope, descriptor, e);

			return Marshall.Convert<R>(result);
		}

		public void Invoke<A>([CAP(CAP.ApiProvider)] string api, A e)
		{
			var descriptor = ComponentDescriptor.Api(this, api);
			var invoker = new ApiInvoker(this);

			invoker.Invoke(this as IApiExecutionScope, descriptor, e);
		}

		public IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)]string connection)
		{
			var tokens = connection.Split('/');
			var descriptor = ComponentDescriptor.Connection(this, connection);

			descriptor.Validate();

			var dataProvider = CreateDataProvider(descriptor.Configuration);

			return dataProvider.OpenConnection(descriptor.Configuration.Value);
		}

		public IDataReader<T> OpenReader<T>(IDataConnection connection, string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter(IDataConnection connection, string commandText)
		{
			return new DataWriter(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText,
				CloseConnection = true
			};
		}

		public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, string commandText)
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
	}
}
