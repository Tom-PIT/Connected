using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT.UI
{
	public interface IViewEngine
	{
		void Render(string name);
		string SnippetPath(ViewContext context, string snippetname);
	}
}
