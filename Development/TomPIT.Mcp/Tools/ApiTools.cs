using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Mcp.Protocol;
using TomPIT.Middleware;

namespace TomPIT.Mcp.Tools;

internal static class ApiTools
{
	internal static object Invoke(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");
		var api = args.Value<string>("api") ?? throw new McpToolException("api is required");
		var operation = args.Value<string>("operation") ?? throw new McpToolException("operation is required");
		var arguments = args["arguments"] as JObject ?? new JObject();

		var ms = Tenant.GetService<IMicroServiceService>().SelectByUrl(msIdentifier)
			?? Tenant.GetService<IMicroServiceService>().Select(msIdentifier)
			?? throw new McpToolException($"MicroService not found: '{msIdentifier}'");

		// Create a microservice context bound to the target service and invoke the operation
		using var ctx = new MicroServiceContext(ms);

		object? result;

		try
		{
			result = ctx.Interop.Invoke<object, JObject>($"{api}/{operation}", arguments);
		}
		catch (Exception ex)
		{
			throw new McpToolException($"API invocation failed: {ex.Message}");
		}

		return new
		{
			microService = ms.Url,
			api,
			operation,
			result
		};
	}

	internal static IEnumerable<McpTool> Definitions() =>
	[
		new McpTool
		{
			Name = "api_invoke",
			Description = "Invoke a TomPIT API operation on the running instance. The API must have Public scope. Arguments are passed as a JSON object matching the operation's input properties.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["microService"] = new() { Type = "string", Description = "Microservice URL slug or name" },
					["api"] = new() { Type = "string", Description = "API component name" },
					["operation"] = new() { Type = "string", Description = "Operation name within the API" },
					["arguments"] = new() { Type = "string", Description = "Optional JSON object of arguments to pass to the operation" }
				},
				Required = ["microService", "api", "operation"]
			}
		}
	];
}
