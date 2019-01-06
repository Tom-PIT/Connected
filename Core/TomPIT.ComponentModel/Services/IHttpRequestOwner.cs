using Microsoft.AspNetCore.Http;

namespace TomPIT.Services
{
	internal interface IHttpRequestOwner
	{
		HttpRequest HttpRequest { get; }
	}
}
