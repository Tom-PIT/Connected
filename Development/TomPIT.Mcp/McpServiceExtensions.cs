using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Mcp;

public static class McpServiceExtensions
{
	/// <summary>
	/// Registers the /mcp endpoint route into the application pipeline.
	/// Call this after app.UseAuthorization() in the host startup.
	/// </summary>
	public static void MapTomPITMcp(this IEndpointRouteBuilder builder)
	{
		builder.MapControllerRoute("mcp.info", "mcp", new { controller = "Mcp", action = "Info" });
		builder.MapControllerRoute("mcp.handle", "mcp", new { controller = "Mcp", action = "Handle" });

		builder.MapControllerRoute("ide.microservices", "sys/ide/microservices", new { controller = "IdeApi", action = "Microservices" });
		builder.MapControllerRoute("ide.components", "sys/ide/components", new { controller = "IdeApi", action = "Components" });
		builder.MapControllerRoute("ide.source", "sys/ide/source", new { controller = "IdeApi", action = "Source" });
	}
}
