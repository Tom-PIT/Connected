using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Ide
{
	public static class IdeRouting
	{
		public static void Register(IEndpointRouteBuilder builder, string controllerName, string pattern)
		{
			builder.MapControllerRoute("ide", pattern, new { controller = controllerName, action = "Index" });
			builder.MapControllerRoute("ide.dom", string.Format("{0}/dom", pattern).Trim('/'), new { controller = controllerName, action = "Dom" });
			builder.MapControllerRoute("ide.section", string.Format("{0}/section", pattern).Trim('/'), new { controller = controllerName, action = "Section" });
			builder.MapControllerRoute("ide.save", string.Format("{0}/save", pattern).Trim('/'), new { controller = controllerName, action = "Save" });
			builder.MapControllerRoute("ide.action", string.Format("{0}/action", pattern).Trim('/'), new { controller = controllerName, action = "Action" });
			builder.MapControllerRoute("ide.upload", string.Format("{0}/upload", pattern).Trim('/'), new { controller = controllerName, action = "Upload" });
			builder.MapControllerRoute("ide.ide", string.Format("{0}/ide", pattern).Trim('/'), new { controller = controllerName, action = "IdeAction" });
		}
	}
}
