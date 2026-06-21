using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Mcp.Protocol;
using TomPIT.Reflection;

namespace TomPIT.Mcp.Tools;

internal static class ConfigurationTools
{
	internal static object ReadConfig(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException("Configuration not found");

		var json = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore
		});

		return new
		{
			component = component.Name,
			category = component.Category,
			nameSpace = component.NameSpace,
			microService = component.MicroService,
			configuration = JToken.Parse(json)
		};
	}

	internal static object ListAddableTypes(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");

		var ms = ResolveMicroService(msIdentifier);
		var template = Tenant.GetService<IMicroServiceTemplateService>().Select(ms.Template);

		var items = template?.ProvideAddItems(null) ?? new();
		var globalItems = template?.ProvideGlobalAddItems(null) ?? new();

		return items.Concat(globalItems)
			.DistinctBy(d => d.Id)
			.OrderBy(d => d.Category).ThenBy(d => d.Text)
			.Select(d => new
			{
				category = d.Value?.ToString() ?? d.Id,
				label = d.Text,
				group = d.Category,
				typeName = d.Type?.TypeName()
			})
			.ToList();
	}

	internal static object CreateComponent(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");
		var category = args.Value<string>("category") ?? throw new McpToolException("category is required");
		var name = args.Value<string>("name") ?? throw new McpToolException("name is required");
		var folderRaw = args.Value<string>("folder");

		var ms = ResolveMicroService(msIdentifier);
		var folder = folderRaw is not null && Guid.TryParse(folderRaw, out var fg) ? fg : Guid.Empty;

		// Resolve type from the microservice's template
		var template = Tenant.GetService<IMicroServiceTemplateService>().Select(ms.Template);
		var allItems = (template?.ProvideAddItems(null) ?? new())
			.Concat(template?.ProvideGlobalAddItems(null) ?? new())
			.ToList();

		var descriptor = allItems.FirstOrDefault(d =>
			string.Equals(d.Value?.ToString(), category, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(d.Id, category, StringComparison.OrdinalIgnoreCase));

		if (descriptor is null)
			throw new McpToolException($"Category '{category}' is not available in this microservice's template. Call component_types_list to see what can be created.");

		var typeName = descriptor.Type?.TypeName()
			?? throw new McpToolException($"Cannot resolve type for category '{category}'");

		var token = Tenant.GetService<IDesignService>().Components.Insert(ms.Token, folder, category, name, typeName);
		var created = Tenant.GetService<IComponentService>().SelectComponent(token);

		return new
		{
			success = true,
			token = created?.Token,
			name,
			category,
			microService = ms.Url
		};
	}

	internal static object DeleteComponent(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		Tenant.GetService<IDesignService>().Components.Delete(componentToken);

		return new { success = true, deleted = component.Name };
	}

	internal static object RenameComponent(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var name = args.Value<string>("name") ?? throw new McpToolException("name is required");
		var folderRaw = args.Value<string>("folder");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		var folder = folderRaw is not null && Guid.TryParse(folderRaw, out var fg) ? fg : component.Folder;

		Tenant.GetService<IDesignService>().Components.Update(componentToken, name, folder);

		return new { success = true, token = componentToken, name, folder };
	}

	internal static object CreateFolder(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");
		var name = args.Value<string>("name") ?? throw new McpToolException("name is required");
		var parentRaw = args.Value<string>("parent");

		var ms = ResolveMicroService(msIdentifier);
		var parent = parentRaw is not null && Guid.TryParse(parentRaw, out var pg) ? pg : Guid.Empty;

		Tenant.GetService<IDesignService>().Components.InsertFolder(ms.Token, name, parent);

		var created = Tenant.GetService<IComponentService>().QueryFolders(ms.Token, parent)
			.LastOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

		return new { success = true, token = created?.Token, name, microService = ms.Url };
	}

	internal static IEnumerable<McpTool> Definitions() =>
	[
		new McpTool
		{
			Name = "component_config_read",
			Description = "Read the full configuration of a component as JSON. Includes type-specific properties such as API operations with scope, model connections, view layout, query operations, etc.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["componentToken"] = new() { Type = "string", Description = "The component GUID token" }
				},
				Required = ["componentToken"]
			}
		},
		new McpTool
		{
			Name = "component_types_list",
			Description = "List all component types that can be created within a microservice. Use this before component_create to find the correct category name.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["microService"] = new() { Type = "string", Description = "Microservice URL slug or name" }
				},
				Required = ["microService"]
			}
		},
		new McpTool
		{
			Name = "component_create",
			Description = "Create a new component in a microservice. Use component_types_list first to get valid category names.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["microService"] = new() { Type = "string", Description = "Microservice URL slug or name" },
					["category"] = new() { Type = "string", Description = "Component category from component_types_list (e.g. 'Api', 'View', 'Script')" },
					["name"] = new() { Type = "string", Description = "Component name" },
					["folder"] = new() { Type = "string", Description = "Optional folder GUID to place the component in" }
				},
				Required = ["microService", "category", "name"]
			}
		},
		new McpTool
		{
			Name = "component_delete",
			Description = "Permanently delete a component and all its source blobs. This action cannot be undone.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["componentToken"] = new() { Type = "string", Description = "The component GUID token" }
				},
				Required = ["componentToken"]
			}
		},
		new McpTool
		{
			Name = "component_rename",
			Description = "Rename a component and/or move it to a different folder.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["componentToken"] = new() { Type = "string", Description = "The component GUID token" },
					["name"] = new() { Type = "string", Description = "New component name" },
					["folder"] = new() { Type = "string", Description = "Optional new folder GUID (omit to keep current folder)" }
				},
				Required = ["componentToken", "name"]
			}
		},
		new McpTool
		{
			Name = "folder_create",
			Description = "Create a folder inside a microservice to organize components.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["microService"] = new() { Type = "string", Description = "Microservice URL slug or name" },
					["name"] = new() { Type = "string", Description = "Folder name" },
					["parent"] = new() { Type = "string", Description = "Optional parent folder GUID (omit for root)" }
				},
				Required = ["microService", "name"]
			}
		}
	];

	private static IMicroService ResolveMicroService(string identifier)
	{
		return Tenant.GetService<IMicroServiceService>().SelectByUrl(identifier)
			?? Tenant.GetService<IMicroServiceService>().Select(identifier)
			?? throw new McpToolException($"MicroService not found: '{identifier}'");
	}

	private static Guid ParseToken(JObject args, string key)
	{
		var raw = args.Value<string>(key) ?? throw new McpToolException($"{key} is required");
		return Guid.TryParse(raw, out var g) ? g : throw new McpToolException($"{key} is not a valid GUID");
	}
}
