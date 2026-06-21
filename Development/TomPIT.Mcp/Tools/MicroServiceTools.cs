using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Mcp.Protocol;
using TomPIT.Reflection;

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

	internal static object GetReferences(JObject args)
	{
		var identifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");

		var ms = Tenant.GetService<IMicroServiceService>().SelectByUrl(identifier)
			?? Tenant.GetService<IMicroServiceService>().Select(identifier)
			?? throw new McpToolException($"MicroService not found: '{identifier}'");

		var refDiscovery = Tenant.GetService<IDiscoveryService>().MicroServices.References;

		var dependsOn = refDiscovery.References(ms.Token, false);
		var usedBy = refDiscovery.ReferencedBy(ms.Token, false);

		return new
		{
			microService = new { token = ms.Token, name = ms.Name, url = ms.Url },
			dependsOn = dependsOn.Select(r => new { token = r.Token, name = r.Name, url = r.Url }).ToList(),
			usedBy = usedBy.Select(r => new { token = r.Token, name = r.Name, url = r.Url }).ToList()
		};
	}

	internal static IEnumerable<McpTool> Definitions()
	{
		return new List<McpTool>
		{
			new McpTool
			{
				Name = "microservice_list",
				Description = "List all microservices registered on this TomPIT instance. Returns token, name, url, version, commit, resourceGroup and template for each.",
				InputSchema = new() { Type = "object", Properties = new Dictionary<string, McpProperty>() }
			},
			new McpTool
			{
				Name = "microservice_references",
				Description = "Get the dependency graph for a microservice: which services it depends on (dependsOn) and which services reference it (usedBy). Cross-service #load directives are only valid when the target is in 'dependsOn'.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["microService"] = new() { Type = "string", Description = "The microservice URL slug or name" }
					},
					Required = new List<string> { "microService" }
				}
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
					Required = new List<string> { "microService" }
				}
			}
		};
	}
}
