using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Services;

namespace TomPIT.UI
{
	public interface IViewEngine
	{
		HttpContext Context { get; set; }
		void Render(string name);
		void RenderPartial(IExecutionContext context, string name);
		string SnippetPath(ViewContext context, string snippetname);
	}
}
