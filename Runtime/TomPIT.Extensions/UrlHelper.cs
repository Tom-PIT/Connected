using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public class UrlHelper : HelperBase
	{
		public UrlHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public HtmlString Relative(string relativeUrl)
		{
			var request = Html.ViewContext.HttpContext.Request;

			var scheme = request.Scheme;
			var host = request.Host;
			var pathBase = request.PathBase;

			return Html.Raw(string.Format("{0}://{1}{2}/{3}", scheme, host, pathBase, relativeUrl)) as HtmlString;
		}
	}
}
