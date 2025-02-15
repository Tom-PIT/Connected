﻿using System.Text;
using TomPIT.ComponentModel.UI;
using TomPIT.Design;
using TomPIT.Middleware;

namespace TomPIT.Handlers
{
	internal class MasterCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IMiddlewareContext context, object instance)
		{
			if (instance is IMasterViewConfiguration m)
			{
				var sb = new StringBuilder();

				sb.AppendLine("<!DOCTYPE html>");
				sb.AppendLine("@model TomPIT.Middleware.IMiddlewareContext");
				sb.AppendLine("<html>");
				sb.AppendLine("<head>");
				sb.AppendLine("<title>@ViewBag.Title</title>");
				sb.AppendLine("@await Html.PartialAsync(\"~/Views/Shared/StandardHeaders.cshtml\")");
				sb.AppendLine("@await Html.PartialAsync(\"~/Views/Shared/Header.cshtml\")");
				sb.AppendLine("@RenderSection(\"head\", false)");
				sb.AppendLine("</head>");
				sb.AppendLine("<body>");
				sb.AppendLine("@await Html.PartialAsync(\"~/Views/Shared/ContentInit.cshtml\", Model)");
				sb.AppendLine("<div class=\"container-fluid sys-container h-100\" id=\"_sysBody\">");
				sb.AppendLine("<div class=\"row content-area no-gutters h-100\">");
				sb.AppendLine("<div class=\"col\" id=\"_sysContent\">");
				sb.AppendLine("@RenderBody()");
				sb.AppendLine("</div>");
				sb.AppendLine("</div>");
				sb.AppendLine("</div>");
				sb.AppendLine("@RenderSection(\"Scripts\", false)");
				sb.AppendLine("<script src=\"~/Assets/scripts.foot.min.js\"></script>");
				sb.AppendLine("</body>");
				sb.AppendLine("</html>");

				context.Tenant.GetService<IDesignService>().Components.Update(m, sb.ToString());
			}
		}
	}
}
