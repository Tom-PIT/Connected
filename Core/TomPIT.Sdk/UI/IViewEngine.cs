using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TomPIT.UI
{
	public interface IViewEngine
	{
		HttpContext Context { get; set; }
		Task Render(string name);
		//Task RenderPartial(IMicroServiceContext context, string name);
		//Task<string> CompilePartial(IMicroServiceContext context, string name);
	}
}
