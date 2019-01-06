using Microsoft.AspNetCore.Http;

namespace TomPIT.Services
{
	public interface IHttpRequestOwner
	{
		HttpRequest HttpRequest { get; }
	}
}
