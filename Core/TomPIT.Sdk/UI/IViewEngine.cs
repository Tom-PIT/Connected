using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Middleware;

namespace TomPIT.UI
{
	public interface IViewEngine
	{
		HttpContext Context { get; set; }
		void Render(string name);
		void RenderPartial(IMicroServiceContext context, string name);
		string SnippetPath(ViewContext context, string snippetname);
	}
}
