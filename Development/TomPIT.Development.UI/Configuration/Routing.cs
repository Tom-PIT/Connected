using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Development.Routing;
using TomPIT.Routing;

namespace TomPIT.Development.Configuration
{
	internal static class Routing
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("home", "", new { controller = "Home", action = "Index" });
			routes.MapControllerRoute("home.tool", "sys/tool/{tool}", new { controller = "Home", action = "Tool" });
			routes.MapControllerRoute("home.runtool", "sys/run-tool/{tool}", new { controller = "Home", action = "Action" });
			routes.MapControllerRoute("home.data", "sys/get-data/{tool}", new { controller = "Home", action = "Data" });
			routes.MapControllerRoute("home.autofix", "sys/auto-fix", new { controller = "Home", action = "AutoFix" });
			routes.MapControllerRoute("sys.apitest", "sys/apitest", new { controller = "ApiTest", action = "Index" });
			routes.MapControllerRoute("sys.apitest.invoke", "sys/apitest/invoke", new { controller = "ApiTest", action = "Invoke" });
			routes.MapControllerRoute("sys.apitest.save", "sys/apitest/save", new { controller = "ApiTest", action = "Save" });
			routes.MapControllerRoute("sys.apitest.querytags", "sys/apitest/querytags", new { controller = "ApiTest", action = "QueryTags" });
			routes.MapControllerRoute("sys.apitest.querytests", "sys/apitest/querytests", new { controller = "ApiTest", action = "QueryTests" });
			routes.MapControllerRoute("sys.apitest.body", "sys/apitest/selectbody", new { controller = "ApiTest", action = "SelectBody" });
			routes.MapControllerRoute("sys.apitest.delete", "sys/apitest/delete", new { controller = "ApiTest", action = "Delete" });
			routes.MapControllerRoute("sys.apitest.provideitems", "sys/apitest/provideitems", new { controller = "ApiTest", action = "ProvideItems" });
			routes.MapControllerRoute("sys.vc", "sys/version-control", new { controller = "VersionControl", action = "Index" });
			routes.MapControllerRoute("sys.vc.designer", "sys/version-control/designer", new { controller = "VersionControl", action = "Designer" });
			routes.MapControllerRoute("sys.vc.changes", "sys/version-control/changes", new { controller = "VersionControl", action = "Changes" });
			routes.MapControllerRoute("sys.vc.diff", "sys/version-control/diff", new { controller = "VersionControl", action = "Diff" });
			//routes.MapControllerRoute("sys.testsuites", "sys/test-suites", new { controller = "TestSuites", action = "Index" });
			//routes.MapControllerRoute("sys.testsuites.select", "sys/test-suites/select", new { controller = "TestSuites", action = "Select" });

			routes.Map("sys/source-code/{microService}/{component}/{template}", (t) =>
			{
				new SourceCode().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.Map("sys/media/{id}/{version}", (t) =>
			{
				new MediaHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}
