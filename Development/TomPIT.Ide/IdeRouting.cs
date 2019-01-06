using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT
{
	public static class IdeRouting
	{
		public static void Register(IRouteBuilder builder, string controllerName, string pattern)
		{
			builder.MapRoute("ide", pattern, new { controller = controllerName, action = "Index" });
			builder.MapRoute("ide.dom", string.Format("{0}/dom", pattern).Trim('/'), new { controller = controllerName, action = "Dom" });
			builder.MapRoute("ide.section", string.Format("{0}/section", pattern).Trim('/'), new { controller = controllerName, action = "Section" });
			builder.MapRoute("ide.save", string.Format("{0}/save", pattern).Trim('/'), new { controller = controllerName, action = "Save" });
			builder.MapRoute("ide.action", string.Format("{0}/action", pattern).Trim('/'), new { controller = controllerName, action = "Action" });
			builder.MapRoute("ide.upload", string.Format("{0}/upload", pattern).Trim('/'), new { controller = controllerName, action = "Upload" });
		}
	}
}
