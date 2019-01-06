using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Runtime
{
	public interface IRequestContextProvider
	{
		HttpRequest Request { get; }
		ActionContext ActionContext { get; }
	}
}
