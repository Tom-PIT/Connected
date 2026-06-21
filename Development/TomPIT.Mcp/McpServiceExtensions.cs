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
	}
}
