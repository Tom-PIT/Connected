using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.Compilation;
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
		internal const string SubscriptionProvider = "TomPIT.Design.CodeAnalysis.Providers.SubscriptionProvider, TomPIT.Design";
		internal const string SubscriptionEventProvider = "TomPIT.Design.CodeAnalysis.Providers.SubscriptionEventProvider, TomPIT.Design";
		internal const string MailTemplateProvider = "TomPIT.Design.CodeAnalysis.Providers.MailTemplateProvider, TomPIT.Design";
		internal const string StringTableProvider = "TomPIT.Design.CodeAnalysis.Providers.StringTableProvider, TomPIT.Design";
		internal const string SearchCatalogProvider = "TomPIT.Design.CodeAnalysis.Providers.SearchCatalogProvider, TomPIT.Design";
		internal const string StringTableStringProvider = "TomPIT.Design.CodeAnalysis.Providers.StringTableStringProvider, TomPIT.Design";
		internal const string MediaProvider = "TomPIT.Design.CodeAnalysis.Providers.MediaProvider, TomPIT.Design";
		internal const string IoTHubProvider = "TomPIT.Design.CodeAnalysis.Providers.IoTHubProvider, TomPIT.Design";
		internal const string IoTHubFieldProvider = "TomPIT.Design.CodeAnalysis.Providers.IoTHubFieldProvider, TomPIT.Design";
		internal const string QueueWorkerProvider = "TomPIT.Design.CodeAnalysis.Providers.QueueWorkerProvider, TomPIT.Design";
		internal const string PartialProvider = "TomPIT.Design.CodeAnalysis.Providers.PartialProvider, TomPIT.Design";
		internal const string BigDataPartitionProvider = "TomPIT.Design.CodeAnalysis.Providers.BigDataPartitionProvider, TomPIT.Design";

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

		public static IExecutionContext Create(string endpoint, IMicroService microService)
		{
			return Create(endpoint, microService, null);
		}

		public static IExecutionContext Create(string endpoint, IMicroService microService, string impersonatedUser)
		{
			var r = new ExecutionContext(endpoint, microService);

			if (!string.IsNullOrWhiteSpace(impersonatedUser))
			{
				var ids = r.Services.Identity as ContextIdentityService;

				ids.ImpersonatedUser = impersonatedUser;
			}

			return r;
		}

		public R Invoke<R>([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e, IApiTransaction transaction)
		{
			var q = new ApiQualifier(this, api);
			var ai = new ApiInvoke(this);

			var r = ai.Execute(this as IApiExecutionScope, q.MicroService.Token, q.Api, q.Operation, e, transaction, q.ExplicitIdentifier);

			return MarshallingConverter.Convert<R>(r);
		}

		public R Invoke<R>([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e)
		{
			return Invoke<R>(api, e, null);
		}

		public R Invoke<R>([CodeAnalysisProvider(ApiProvider)]string api, IApiTransaction transaction)
		{
			return Invoke<R>(api, null, transaction);
		}

		public R Invoke<R>([CodeAnalysisProvider(ApiProvider)]string api)
		{
			return Invoke<R>(api, null, null);
		}

		public R Invoke<R, A>([CodeAnalysisProvider(ApiProvider)] string api, A e, IApiTransaction transaction)
		{
			return Invoke<R>(api, Types.Deserialize<JObject>(Types.Serialize(e)), transaction);
		}

		public R Invoke<R, A>([CodeAnalysisProvider(ApiProvider)] string api, A e)
		{
			return Invoke<R>(api, Types.Deserialize<JObject>(Types.Serialize(e)));
		}

		public void Invoke<A>([CodeAnalysisProvider(ApiProvider)] string api, A e)
		{
			Invoke<JObject>(api, Types.Deserialize<JObject>(Types.Serialize(e)));
		}

		[Obsolete("Please use strongly typed overloads of the Invoke.")]
		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e, IApiTransaction transaction)
		{
			var q = new ApiQualifier(this, api);
			var ai = new ApiInvoke(this);

			var result = ai.Execute(this as IApiExecutionScope, q.MicroService.Token, q.Api, q.Operation, e, transaction, q.ExplicitIdentifier);

			if (result is JObject)
				return result as JObject;

			return Types.Deserialize<JObject>(Types.Serialize(result));
		}
		[Obsolete("Please use strongly typed overloads of the Invoke.")]
		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api,
			[CodeAnalysisProvider(ApiParameterProvider)]JObject e)
		{
			return Invoke(api, e, null);
		}
		[Obsolete("Please use strongly typed overloads of the Invoke.")]
		public JObject Invoke([CodeAnalysisProvider(ApiProvider)]string api, IApiTransaction transaction)
		{
			return Invoke(api, null, transaction);
		}
		[Obsolete("Please use strongly typed overloads of the Invoke.")]
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
