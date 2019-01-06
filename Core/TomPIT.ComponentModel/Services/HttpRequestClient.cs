using Microsoft.AspNetCore.Http;

namespace TomPIT.Services
{
	internal class HttpRequestClient : ContextClient
	{
		public HttpRequestClient(IExecutionContext context, IHttpRequestOwner request) : base(context)
		{
			Owner = request;
		}

		protected IHttpRequestOwner Owner { get; }

		protected HttpRequest Request
		{
			get { return Owner?.HttpRequest; }
		}
	}
}
