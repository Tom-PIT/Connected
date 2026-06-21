using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Mcp.Protocol;

namespace TomPIT.Mcp.Tools;

internal static class MicroServiceTools
{
	internal static object List()
	{
		var services = Tenant.GetService<IMicroServiceService>().Query();

		return services.Select(ms => new
		{
			token = ms.Token,
			name = ms.Name,
			url = ms.Url,
			version = ms.Version,
			commit = ms.Commit,
			resourceGroup = ms.ResourceGroup,
			template = ms.Template
		}).ToList();
	}

	internal static object? Get(JObject args)
	{
		var identifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");

		var ms = Tenant.GetService<IMicroServiceService>().SelectByUrl(identifier)
			?? Tenant.GetService<IMicroServiceService>().Select(identifier);

		if (ms is null)
			return null;

		return new
		{
			token = ms.Token,
			name = ms.Name,
			url = ms.Url,
			version = ms.Version,
			commit = ms.Commit,
			resourceGroup = ms.ResourceGroup,
			template = ms.Template
		};
	}

	internal static IEnumerable<McpTool> Definitions() =>
	[
		new McpTool
		{
			Name = "microservice_list",
			Description = "List all microservices registered on this TomPIT instance. Returns token, name, url, version, commit, resourceGroup and template for each.",
			InputSchema = new() { Type = "object", Properties = [] }
		},
		new McpTool
		{
			Name = "microservice_get",
			Description = "Get details for a single microservice by its URL slug or name.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["microService"] = new() { Type = "string", Description = "The microservice URL slug or name (e.g. 'acme' or 'Acme Orders')" }
				},
				Required = ["microService"]
			}
		}
	];
}
