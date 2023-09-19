using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TomPIT.Middleware;
using TomPIT.Models;

namespace TomPIT.UI
{
	public interface IViewEngine
	{
		HttpContext Context { get; set; }
		Task Render(string name);
		//Task RenderPartial(IMicroServiceContext context, string name);
		Task<string> RenderPartialToStringAsync(IMicroServiceContext context, string name);
	}
}
