using Newtonsoft.Json;
using System.Collections.Generic;

namespace TomPIT.Mcp.Protocol;

public class McpServerInfo
{
	[JsonProperty("name")]
	public string Name { get; set; } = "TomPIT.connected MCP";

	[JsonProperty("version")]
	public string Version { get; set; } = "1.0.0";
}

public class McpCapabilities
{
	[JsonProperty("tools")]
	public McpToolsCapability? Tools { get; set; }

	[JsonProperty("resources")]
	public McpResourcesCapability? Resources { get; set; }

	[JsonProperty("prompts")]
	public McpPromptsCapability? Prompts { get; set; }
}

public class McpToolsCapability
{
	[JsonProperty("listChanged")]
	public bool ListChanged { get; set; } = false;
}

public class McpResourcesCapability
{
	[JsonProperty("subscribe")]
	public bool Subscribe { get; set; } = false;

	[JsonProperty("listChanged")]
	public bool ListChanged { get; set; } = false;
}

public class McpPromptsCapability
{
	[JsonProperty("listChanged")]
	public bool ListChanged { get; set; } = false;
}

public class McpInitializeResult
{
	[JsonProperty("protocolVersion")]
	public string ProtocolVersion { get; set; } = "2024-11-05";

	[JsonProperty("serverInfo")]
	public McpServerInfo ServerInfo { get; set; } = new();

	[JsonProperty("capabilities")]
	public McpCapabilities Capabilities { get; set; } = new();
}

public class McpTool
{
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	[JsonProperty("inputSchema")]
	public McpInputSchema InputSchema { get; set; } = new();
}

public class McpInputSchema
{
	[JsonProperty("type")]
	public string Type { get; set; } = "object";

	[JsonProperty("properties")]
	public Dictionary<string, McpProperty> Properties { get; set; } = new();

	[JsonProperty("required")]
	public List<string>? Required { get; set; }
}

public class McpProperty
{
	[JsonProperty("type")]
	public string Type { get; set; } = "string";

	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	[JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? Enum { get; set; }
}

public class McpToolsListResult
{
	[JsonProperty("tools")]
	public List<McpTool> Tools { get; set; } = new();
}

public class McpToolCallResult
{
	[JsonProperty("content")]
	public List<McpContent> Content { get; set; } = new();

	[JsonProperty("isError", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsError { get; set; }
}

public class McpContent
{
	[JsonProperty("type")]
	public string Type { get; set; } = "text";

	[JsonProperty("text")]
	public string Text { get; set; } = string.Empty;
}

public class McpResource
{
	[JsonProperty("uri")]
	public string Uri { get; set; } = string.Empty;

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	[JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
	public string? MimeType { get; set; }
}

public class McpResourcesListResult
{
	[JsonProperty("resources")]
	public List<McpResource> Resources { get; set; } = new();
}

public class McpResourceReadResult
{
	[JsonProperty("contents")]
	public List<McpResourceContent> Contents { get; set; } = new();
}

public class McpResourceContent
{
	[JsonProperty("uri")]
	public string Uri { get; set; } = string.Empty;

	[JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
	public string? MimeType { get; set; }

	[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
	public string? Text { get; set; }
}

public class McpPromptsListResult
{
	[JsonProperty("prompts")]
	public List<McpPrompt> Prompts { get; set; } = new();
}

public class McpPrompt
{
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	[JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
	public List<McpPromptArgument>? Arguments { get; set; }
}

public class McpPromptArgument
{
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	[JsonProperty("required")]
	public bool Required { get; set; }
}

public class McpPromptGetResult
{
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	[JsonProperty("messages")]
	public List<McpPromptMessage> Messages { get; set; } = new();
}

public class McpPromptMessage
{
	[JsonProperty("role")]
	public string Role { get; set; } = "user";

	[JsonProperty("content")]
	public McpContent Content { get; set; } = new();
}
