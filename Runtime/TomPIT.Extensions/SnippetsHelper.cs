using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Runtime;

namespace TomPIT
{
	public class SnippetsHelper : HelperBase
	{
		public SnippetsHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public IHtmlContent Render(string name)
		{
			var ve = Html.ViewContext.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

			return Html.Partial(ve.SnippetPath(Html.ViewContext, name));
		}
	}
}
