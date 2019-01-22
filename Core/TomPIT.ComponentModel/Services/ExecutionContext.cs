using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public class ExecutionContext : IExecutionContext, IEndpointContext
	{
		private const string ApiProvider = "TomPIT.Design.CodeAnalysis.Providers.ApiProvider, TomPIT.Design";
		private const string ApiParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.ApiParameterProvider, TomPIT.Design";

		[ThreadStatic]
		private static HttpRequest _currentRequest = null;


		private IContextServices _services = null;
		private IContextIdentity _identity = null;
		private ISysConnection _connection = null;

		public ExecutionContext(IExecutionContext sender)
		{
			var request = sender is IRequestContextProvider r ? r.Request : null;
			var endpoint = sender is IEndpointContext c ? c.Endpoint : null;

			Initialize(request, endpoint, sender.Identity.Authority, sender.Identity.AuthorityId, sender.Identity.ContextId);
		}

		public ExecutionContext(HttpRequest request, string endpoint)
		{
			Initialize(request, endpoint, null, null, null);
		}

		protected ExecutionContext()
		{

		}

		protected void Initialize(HttpRequest request, string endpoint, string authority, string authorityId, string contextId)
		{
			_currentRequest = request;

			Request = request;
			Endpoint = endpoint;

			var ci = Identity as ContextIdentity;

			ci.Authority = authority;
			ci.AuthorityId = authorityId;
			ci.ContextId = contextId;
		}

		public static HttpRequest HttpRequest { get { return _currentRequest; } }

		public HttpRequest Request { get; protected set; }

		public virtual IContextServices Services
		{
			get
			{
				if (_services == null)
					_services = new ContextServices(this, Request);

				return _services;
			}
		}


		public virtual IContextIdentity Identity
		{
			get
			{
				if (_identity == null)
					_identity = new ContextIdentity();

				return _identity;
			}
		}

		public string Endpoint { get; protected set; }

		protected T GetService<T>()
		{
			if (Connection == null)
				throw new ExecutionException(SR.ErrInstanceEndpointNotFound);

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

		public static IExecutionContext NonHttpContext(string endpoint, string authority, string authorityId, string contextId)
		{
			return NonHttpContext(endpoint, authority, authorityId, contextId, string.Empty);
		}

		public static IExecutionContext NonHttpContext(string endpoint, string authority, string authorityId, string contextId, string user)
		{
			var r = new ExecutionContext(null, endpoint);

			var i = r.Identity as ContextIdentity;

			i.Authority = authority;
			i.AuthorityId = authorityId;
			i.ContextId = contextId;

			var ids = r.Services.Identity as ContextIdentityService;

			ids.ImpersonatedUser = user;

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
	}
}
