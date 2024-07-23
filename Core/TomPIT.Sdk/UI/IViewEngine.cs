using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.UI
{
	public interface IViewEngine
	{
		HttpContext Context { get; set; }
		Task<bool> Render(string name);
		//Task RenderPartial(IMicroServiceContext context, string name);
		Task<string> RenderPartialToStringAsync(IMicroServiceContext context, string name);
	}
}
