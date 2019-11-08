using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Models
{
	public interface IActionContextProvider
	{
		//HttpRequest Request { get; }
		ActionContext ActionContext { get; }
	}
}
