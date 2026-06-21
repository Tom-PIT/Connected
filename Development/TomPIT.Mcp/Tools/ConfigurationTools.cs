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

	internal static object RenameFolder(JObject args)
	{
		var folderToken = ParseToken(args, "folder");
		var name = args.Value<string>("name") ?? throw new McpToolException("name is required");
		var parentRaw = args.Value<string>("parent");

		var folder = Tenant.GetService<IComponentService>().SelectFolder(folderToken)
			?? throw new McpToolException($"Folder not found: {folderToken}");

		var parent = parentRaw is not null && Guid.TryParse(parentRaw, out var pg) ? pg : folder.Parent;

		Tenant.GetService<IDesignService>().Components.UpdateFolder(folder.MicroService, folderToken, name, parent);

		return new { success = true, token = folderToken, name };
	}

	internal static object DeleteFolder(JObject args)
	{
		var folderToken = ParseToken(args, "folder");
		var deleteComponents = args.Value<bool?>("deleteComponents") ?? false;

		var folder = Tenant.GetService<IComponentService>().SelectFolder(folderToken)
			?? throw new McpToolException($"Folder not found: {folderToken}");

		Tenant.GetService<IDesignService>().Components.DeleteFolder(folder.MicroService, folderToken, deleteComponents);

		return new { success = true, token = folderToken, name = folder.Name, deleteComponents };
	}

	internal static object CloneComponent(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var msIdentifier = args.Value<string>("microService");
		var folderRaw = args.Value<string>("folder");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		var targetMs = msIdentifier is not null
			? ResolveMicroService(msIdentifier).Token
			: component.MicroService;

		var targetFolder = folderRaw is not null && Guid.TryParse(folderRaw, out var fg) ? fg : component.Folder;

		var newToken = Tenant.GetService<IDesignService>().Components.Clone(componentToken, targetMs, targetFolder);
		var created = Tenant.GetService<IComponentService>().SelectComponent(newToken);

		return new { success = true, token = newToken, name = created?.Name, category = component.Category };
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

	internal static IEnumerable<McpTool> Definitions()
	{
		return new List<McpTool>
		{
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
					Required = new List<string> { "componentToken" }
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
					Required = new List<string> { "microService" }
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
					Required = new List<string> { "microService", "category", "name" }
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
					Required = new List<string> { "componentToken" }
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
					Required = new List<string> { "componentToken", "name" }
				}
			},
			new McpTool
			{
				Name = "folder_rename",
				Description = "Rename a folder and/or move it to a different parent folder.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["folder"] = new() { Type = "string", Description = "Folder GUID token (from folder_list)" },
						["name"] = new() { Type = "string", Description = "New folder name" },
						["parent"] = new() { Type = "string", Description = "Optional new parent folder GUID (omit to keep current parent)" }
					},
					Required = new List<string> { "folder", "name" }
				}
			},
			new McpTool
			{
				Name = "folder_delete",
				Description = "Delete a folder. By default only deletes the folder if empty; set deleteComponents to true to also delete all components inside.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["folder"] = new() { Type = "string", Description = "Folder GUID token (from folder_list)" },
						["deleteComponents"] = new() { Type = "boolean", Description = "If true, also delete all components inside the folder (default: false)" }
					},
					Required = new List<string> { "folder" }
				}
			},
			new McpTool
			{
				Name = "component_clone",
				Description = "Clone (duplicate) an existing component, optionally into a different microservice or folder. The clone gets an auto-generated name.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "GUID token of the component to clone" },
						["microService"] = new() { Type = "string", Description = "Optional target microservice URL slug or name (default: same microservice)" },
						["folder"] = new() { Type = "string", Description = "Optional target folder GUID (default: same folder as original)" }
					},
					Required = new List<string> { "componentToken" }
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
					Required = new List<string> { "microService", "name" }
				}
			}
		};
	}

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
