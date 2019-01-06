using Microsoft.AspNetCore.Http;

namespace TomPIT.Runtime
{
	internal class HttpRequestClient : ApplicationContextClient
	{
		public HttpRequestClient(IApplicationContext context, IHttpRequestOwner request) : base(context)
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
