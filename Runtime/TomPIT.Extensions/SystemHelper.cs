using System;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Middleware;

namespace TomPIT
{
	public class SystemHelper : HelperBase
	{
		public SystemHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public IHtmlContent Avatar(Guid user, string alt, string css)
		{
			if (user == Guid.Empty)
				return DefaultAvatar();

			if (!(Html.ViewData.Model is IMiddlewareContext m))
				return DefaultAvatar();

			var url = m.Services.Routing.Avatar(user);

			if (string.IsNullOrWhiteSpace(url))
				return DefaultAvatar();

			var sb = new StringBuilder();

			sb.AppendFormat("<img src=\"{0}\"", url);

			if (!string.IsNullOrWhiteSpace(alt))
				sb.AppendFormat(" alt=\"{0}\"", alt);

			if (!string.IsNullOrWhiteSpace(css))
				sb.AppendFormat(" class=\"{0}\"", css);

			sb.AppendFormat("/>");

			return Html.Raw(sb.ToString()) as HtmlString;
		}

		private IHtmlContent DefaultAvatar()
		{
			return Html.Raw("<i class=\"fas fa-user-circle\"></i>") as HtmlString;
		}
	}
}
