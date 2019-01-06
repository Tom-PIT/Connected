using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Services
{
	public interface IRequestContextProvider
	{
		HttpRequest Request { get; }
		ActionContext ActionContext { get; }
	}
}
