using System.Collections.Generic;
using TomPIT.Mcp.Protocol;

namespace TomPIT.Mcp.Tools;

internal static class HelpTools
{
	internal static object GetUrls(string baseUrl) => new
	{
		toolDescriptions = $"{baseUrl}/mcp/tool-descriptions",
		guide = $"{baseUrl}/mcp/guide"
	};

	internal static IEnumerable<McpTool> Definitions()
	{
		return new List<McpTool>
		{
			new McpTool
			{
				Name = "tool_help",
				Description = "Returns URLs for the tool descriptions reference and the platform guide. Call this at the start of a session to discover what all tools do.",
				InputSchema = new() { Type = "object", Properties = new() }
			}
		};
	}
}
