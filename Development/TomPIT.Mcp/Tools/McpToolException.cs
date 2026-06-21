using System;

namespace TomPIT.Mcp.Tools;

public class McpToolException : Exception
{
	public McpToolException(string message) : base(message) { }
}
