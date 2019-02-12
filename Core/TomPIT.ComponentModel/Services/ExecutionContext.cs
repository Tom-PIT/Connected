using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public class ExecutionContext : IExecutionContext, IEndpointContext
	{
		internal const string ApiProvider = "TomPIT.Design.CodeAnalysis.Providers.ApiProvider, TomPIT.Design";
		internal const string ApiParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.ApiParameterProvider, TomPIT.Design";

		private IContextServices _services = null;
		private ISysConnection _connection = null;

		public ExecutionContext(IExecutionContext sender, IMicroService microService)
		{
			var endpoint = sender is IEndpointContext c ? c.Endpoint : null;

			Initialize(endpoint, microService);

			if (sender is ExecutionContext ec)
				((ContextDiagnosticService)Services.Diagnostic).MetricParent = ((ContextDiagnosticService)ec.Services.Diagnostic).MetricParent;
		}

		public ExecutionContext(IExecutionContext sender)
		{
			var endpoint = sender is IEndpointContext c ? c.Endpoint : null;

			Initialize(endpoint, sender.MicroService);

			if (sender is ExecutionContext ec)
				((ContextDiagnosticService)Services.Diagnostic).MetricParent = ((ContextDiagnosticService)ec.Services.Diagnostic).MetricParent;
		}

		public ExecutionContext(string endpoint)
		{
			Initialize(endpoint, null);
		}

		public ExecutionContext(string endpoint, IMicroService microService)
		{
			Initialize(endpoint, microService);
		}

		public ExecutionContext(IMicroService microService)
		{
			Initialize(null, microService);
		}

		protected ExecutionContext()
		{

		}

		protected void Initialize(string endpoint, IMicroService microService)
		{
			Endpoint = endpoint;
			MicroService = microService;
		}

		public virtual IContextServices Services
		{
			get
			{
				if (_services == null)
					_services = new ContextServices(this);

				return _services;
			}
		}


		public virtual IMicroService MicroService { get; protected set; }

		public string Endpoint { get; protected set; }

		public T GetService<T>()
		{
			if (Connection == null)
				throw new RuntimeException(SR.ErrInstanceEndpointNotFound).WithMetrics(this);

			return Connection.GetService<T>();
		}

		public ISysConnection Connection
		{
			get
			{
				if (_connection == null)
				{
					if (!string.IsNullOrWhiteSpace(Endpoint))
						_connection = Shell.GetService<IConnectivityService>().Select(Endpoint);
					else
						_connection = this.Connection();
				}

				return _connection;
			}
		}

		public static IExecutionContext NonHttpContext(string endpoint, IMicroService microService, string impersonatedUser)
		{
			var r = new ExecutionContext(endpoint, microService);

			var ids = r.Services.Identity as ContextIdentityService;

			ids.ImpersonatedUser = impersonatedUser;

			return r;
		}

		public T Invoke<T>([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e, IApiTransaction transaction)
		{
			var q = new ApiQualifier(this, api);
			var ai = new ApiInvoke(this);

			var r = ai.Execute(this as IApiExecutionScope, q.MicroService.Token, q.Api, q.Operation, e, transaction, q.ExplicitIdentifier);

			return Types.Convert<T>(r);
		}

		public T Invoke<T>([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e)
		{
			return Invoke<T>(api, e, null);
		}

		public T Invoke<T>([CodeAnalysisProvider(ApiProvider)]string api, IApiTransaction transaction)
		{
			return Invoke<T>(api, null, transaction);
		}

		public T Invoke<T>([CodeAnalysisProvider(ApiProvider)]string api)
		{
			return Invoke<T>(api, null, null);
		}

		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e, IApiTransaction transaction)
		{
			var q = new ApiQualifier(this, api);
			var ai = new ApiInvoke(this);

			return ai.Execute(this as IApiExecutionScope, q.MicroService.Token, q.Api, q.Operation, e, transaction, q.ExplicitIdentifier) as JObject;
		}

		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e)
		{
			return Invoke(api, e, null);
		}

		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api, IApiTransaction transaction)
		{
			return Invoke(api, null, transaction);
		}

		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api)
		{
			return Invoke(api, null, null);
		}

		public RuntimeException Exception(string message)
		{
			return Exception(message, 0);
		}

		public RuntimeException Exception(string format, string message)
		{
			return Exception(format, message, 0);
		}

		public RuntimeException Exception(string message, int eventId)
		{
			return new RuntimeException(ExceptionSource, message)
			{
				Event = eventId
			};
		}

		public RuntimeException Exception(string format, string message, int eventId)
		{
			return new RuntimeException(ExceptionSource, string.Format("{0}", message))
			{
				Event = eventId
			};
		}

		protected virtual string ExceptionSource { get { return GetType().ShortName(); } }
	}
}
