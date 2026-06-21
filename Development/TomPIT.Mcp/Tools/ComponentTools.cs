using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Mcp.Protocol;
using TomPIT.Reflection;

namespace TomPIT.Mcp.Tools;

internal static class ComponentTools
{
	internal static object ListComponents(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");
		var category = args.Value<string>("category");

		var ms = ResolveMicroService(msIdentifier);

		var components = string.IsNullOrWhiteSpace(category)
			? Tenant.GetService<IComponentService>().QueryComponents(ms.Token)
			: Tenant.GetService<IComponentService>().QueryComponents(ms.Token, category);

		return components.Select(c => new
		{
			token = c.Token,
			name = c.Name,
			category = c.Category,
			nameSpace = c.NameSpace,
			folder = c.Folder == Guid.Empty ? (Guid?)null : c.Folder,
			modified = c.Modified,
			microService = ms.Url
		}).OrderBy(c => c.category).ThenBy(c => c.name).ToList();
	}

	internal static object ListFolders(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");
		var ms = ResolveMicroService(msIdentifier);

		var folders = Tenant.GetService<IComponentService>().QueryFolders(ms.Token);

		return BuildFolderTree(folders, Guid.Empty);
	}

	internal static object ReadSource(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var elementName = args.Value<string>("elementName");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		var ms = Tenant.GetService<IMicroServiceService>().Select(component.MicroService)
			?? throw new McpToolException("MicroService not found");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException("Configuration not found");

		var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

		if (!texts.Any())
			return new { component = component.Name, category = component.Category, sources = Array.Empty<object>() };

		if (!string.IsNullOrWhiteSpace(elementName))
		{
			var text = texts.FirstOrDefault(t =>
				string.Equals(System.IO.Path.GetFileNameWithoutExtension(t.FileName), elementName, StringComparison.OrdinalIgnoreCase))
				?? throw new McpToolException($"Element '{elementName}' not found in component '{component.Name}'");

			var source = Tenant.GetService<IComponentService>().SelectText(ms.Token, text);
			return new { component = component.Name, category = component.Category, elementName, source };
		}

		var sources = texts.Select(t => new
		{
			elementName = System.IO.Path.GetFileNameWithoutExtension(t.FileName),
			fileName = t.FileName,
			source = Tenant.GetService<IComponentService>().SelectText(ms.Token, t)
		}).ToList();

		return new { component = component.Name, category = component.Category, sources };
	}

	internal static object WriteSource(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var content = args.Value<string>("content") ?? throw new McpToolException("content is required");
		var elementName = args.Value<string>("elementName");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException("Configuration not found");

		var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

		IText? target;

		if (!string.IsNullOrWhiteSpace(elementName))
		{
			target = texts.FirstOrDefault(t =>
				string.Equals(System.IO.Path.GetFileNameWithoutExtension(t.FileName), elementName, StringComparison.OrdinalIgnoreCase))
				?? throw new McpToolException($"Element '{elementName}' not found in component '{component.Name}'");
		}
		else if (texts.Count == 1)
		{
			target = texts[0];
		}
		else if (texts.Count > 1)
		{
			throw new McpToolException($"Component '{component.Name}' has multiple source elements. Specify 'elementName'. Available: {string.Join(", ", texts.Select(t => System.IO.Path.GetFileNameWithoutExtension(t.FileName)))}");
		}
		else
		{
			throw new McpToolException($"Component '{component.Name}' has no source elements");
		}

		Tenant.GetService<IDesignService>().Components.Update(target, content);

		return new { success = true, component = component.Name, elementName = System.IO.Path.GetFileNameWithoutExtension(target.FileName) };
	}

	internal static IEnumerable<McpTool> Definitions() =>
	[
		new McpTool
		{
			Name = "component_list",
			Description = "List components in a microservice. Optionally filter by category (Api, View, Script, Model, MasterView, Partial, Queue, Subscription, etc.).",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["microService"] = new() { Type = "string", Description = "Microservice URL slug or name" },
					["category"] = new() { Type = "string", Description = "Optional category filter (e.g. 'Api', 'View', 'Script', 'Model')" }
				},
				Required = ["microService"]
			}
		},
		new McpTool
		{
			Name = "folder_list",
			Description = "Get the folder hierarchy for a microservice. Returns a nested tree structure.",
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
			Name = "component_source_read",
			Description = "Read the source code of a component. For components with multiple elements (e.g. API operations, model queries), all elements are returned unless 'elementName' is specified.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["componentToken"] = new() { Type = "string", Description = "The component GUID token" },
					["elementName"] = new() { Type = "string", Description = "Optional: name of a specific element (e.g. API operation name, file name without extension)" }
				},
				Required = ["componentToken"]
			}
		},
		new McpTool
		{
			Name = "component_source_write",
			Description = "Write (update) the source code of a component or a specific element within it. Triggers recompilation automatically.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["componentToken"] = new() { Type = "string", Description = "The component GUID token" },
					["content"] = new() { Type = "string", Description = "The new source code content" },
					["elementName"] = new() { Type = "string", Description = "Required when the component has multiple elements (e.g. API operation name)" }
				},
				Required = ["componentToken", "content"]
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

	private static List<object> BuildFolderTree(System.Collections.Immutable.ImmutableList<IFolder> all, Guid parentId)
	{
		return all
			.Where(f => f.Parent == parentId)
			.Select(f => (object)new
			{
				token = f.Token,
				name = f.Name,
				children = BuildFolderTree(all, f.Token)
			})
			.ToList();
	}
}
