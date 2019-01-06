using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Net;
using TomPIT.Runtime.ApplicationContextServices;

namespace TomPIT.Runtime
{
	public class ApplicationContext : IApplicationContext, IEndpointContext
	{
		private IServices _services = null;
		private IContextIdentity _identity = null;
		private ISysContext _context = null;

		public ApplicationContext(IApplicationContext sender)
		{
			var request = sender is IRequestContextProvider r ? r.Request : null;
			var endpoint = sender is IEndpointContext c ? c.Endpoint : null;

			Initialize(request, endpoint, sender.Identity.Authority, sender.Identity.AuthorityId, sender.Identity.ContextId);
		}

		public ApplicationContext(HttpRequest request, string endpoint)
		{
			Initialize(request, endpoint, null, null, null);
		}

		protected ApplicationContext()
		{

		}

		protected void Initialize(HttpRequest request, string endpoint, string authority, string authorityId, string contextId)
		{
			Request = request;
			Endpoint = endpoint;

			var ci = Identity as ContextIdentity;

			ci.Authority = authority;
			ci.AuthorityId = authorityId;
			ci.ContextId = contextId;
		}

		public HttpRequest Request { get; protected set; }

		public virtual IServices Services
		{
			get
			{
				if (_services == null)
					_services = new Services(this, Request);

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
			if (SysContext == null)
				throw new RuntimeException(SR.ErrInstanceEndpointNotFound);

			return SysContext.GetService<T>();
		}

		public ISysContext SysContext
		{
			get
			{
				if (_context == null)
				{
					if (!string.IsNullOrWhiteSpace(Endpoint))
						_context = Shell.GetService<IConnectivityService>().Select(Endpoint);
					else
						_context = this.GetServerContext();
				}

				return _context;
			}
		}

		public static IApplicationContext NonHttpContext(string endpoint, string authority, string authorityId, string contextId)
		{
			return NonHttpContext(endpoint, authority, authorityId, contextId, string.Empty);
		}

		public static IApplicationContext NonHttpContext(string endpoint, string authority, string authorityId, string contextId, string user)
		{
			var r = new ApplicationContext(null, endpoint);

			var i = r.Identity as ContextIdentity;

			i.Authority = authority;
			i.AuthorityId = authorityId;
			i.ContextId = contextId;

			var ids = r.Services.Identity as IdentityService;

			ids.ImpersonatedUser = user;

			return r;
		}

		public T Invoke<T>([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api,
			[CodeAnalysisProvider("TomPIT.Design.ApiParameterProvider, TomPIT.Development")]JObject e, IApiTransaction transaction)
		{
			var q = new ApiQualifier(this, api);
			var ai = new ApiInvoke(this);

			var r = ai.Execute(this as IApiExecutionScope, q.MicroService.Token, q.Api, q.Operation, e, transaction, q.ExplicitIdentifier);

			return Types.Convert<T>(r);
		}

		public T Invoke<T>([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api,
			[CodeAnalysisProvider("TomPIT.Design.ApiParameterProvider, TomPIT.Development")]JObject e)
		{
			return Invoke<T>(api, e, null);
		}

		public T Invoke<T>([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api, IApiTransaction transaction)
		{
			return Invoke<T>(api, null, transaction);
		}

		public T Invoke<T>([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api)
		{
			return Invoke<T>(api, null, null);
		}

		public JObject Invoke([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api,
			[CodeAnalysisProvider("TomPIT.Design.ApiParameterProvider, TomPIT.Development")]JObject e, IApiTransaction transaction)
		{
			var q = new ApiQualifier(this, api);
			var ai = new ApiInvoke(this);

			return ai.Execute(this as IApiExecutionScope, q.MicroService.Token, q.Api, q.Operation, e, transaction, q.ExplicitIdentifier) as JObject;
		}

		public JObject Invoke([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api,
			[CodeAnalysisProvider("TomPIT.Design.ApiParameterProvider, TomPIT.Development")]JObject e)
		{
			return Invoke(api, e, null);
		}

		public JObject Invoke([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api, IApiTransaction transaction)
		{
			return Invoke(api, null, transaction);
		}

		public JObject Invoke([CodeAnalysisProvider("TomPIT.Design.ApiProvider, TomPIT.Development")]string api)
		{
			return Invoke(api, null, null);
		}
	}
}
