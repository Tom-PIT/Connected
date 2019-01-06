using Microsoft.AspNetCore.Http;

namespace TomPIT.Runtime
{
	internal interface IHttpRequestOwner
	{
		HttpRequest HttpRequest { get; }
	}
}
