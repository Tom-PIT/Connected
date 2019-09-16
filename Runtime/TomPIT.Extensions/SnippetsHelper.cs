using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.UI;

namespace TomPIT
{
	public class SnippetsHelper : HelperBase
	{
		public SnippetsHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name)
		{
			var ve = Html.ViewContext.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

			return await Html.PartialAsync(ve.SnippetPath(Html.ViewContext, name));
		}
	}
}
