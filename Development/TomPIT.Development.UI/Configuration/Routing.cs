using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Development.Routing;
using TomPIT.Routing;

namespace TomPIT.Development.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("home", "", new { controller = "Home", action = "Index" });
			routes.MapRoute("home.tool", "sys/tool/{tool}", new { controller = "Home", action = "Tool" });
			routes.MapRoute("home.runtool", "sys/run-tool/{tool}", new { controller = "Home", action = "Action" });
			routes.MapRoute("home.data", "sys/get-data/{tool}", new { controller = "Home", action = "Data" });
			routes.MapRoute("home.autofix", "sys/auto-fix", new { controller = "Home", action = "AutoFix" });
			routes.MapRoute("sys.apitest", "sys/apitest", new { controller = "ApiTest", action = "Index" });
			routes.MapRoute("sys.apitest.invoke", "sys/apitest/invoke", new { controller = "ApiTest", action = "Invoke" });
			routes.MapRoute("sys.apitest.save", "sys/apitest/save", new { controller = "ApiTest", action = "Save" });
			routes.MapRoute("sys.apitest.querytags", "sys/apitest/querytags", new { controller = "ApiTest", action = "QueryTags" });
			routes.MapRoute("sys.apitest.querytests", "sys/apitest/querytests", new { controller = "ApiTest", action = "QueryTests" });
			routes.MapRoute("sys.apitest.body", "sys/apitest/selectbody", new { controller = "ApiTest", action = "SelectBody" });
			routes.MapRoute("sys.apitest.delete", "sys/apitest/delete", new { controller = "ApiTest", action = "Delete" });
			routes.MapRoute("sys.apitest.provideitems", "sys/apitest/provideitems", new { controller = "ApiTest", action = "ProvideItems" });
			routes.MapRoute("sys.testsuites", "sys/test-suites", new { controller = "TestSuites", action = "Index" });
			routes.MapRoute("sys.testsuites.select", "sys/test-suites/select", new { controller = "TestSuites", action = "Select" });

			routes.MapRoute("sys/source-code/{microService}/{component}/{template}", (t) =>
			{
				new SourceCode().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/media/{id}/{version}", (t) =>
			{
				new MediaHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}
