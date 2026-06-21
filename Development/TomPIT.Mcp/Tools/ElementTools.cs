using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Mcp.Protocol;

namespace TomPIT.Mcp.Tools;

internal static class ElementTools
{
	internal static object CreateElement(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var collectionProperty = args.Value<string>("collectionProperty")
			?? throw new McpToolException("collectionProperty is required");
		var elementTypeName = args.Value<string>("elementType");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException($"Configuration not found for component: {componentToken}");

		var prop = config.GetType().GetProperty(collectionProperty, BindingFlags.Public | BindingFlags.Instance)
			?? throw new McpToolException($"Property '{collectionProperty}' not found on '{config.GetType().Name}'. Use component_config_read to see available properties.");

		var collection = prop.GetValue(config)
			?? throw new McpToolException($"Collection '{collectionProperty}' is null");

		var elementType = ResolveElementType(collection.GetType(), (IEnumerable)collection, elementTypeName);

		var instance = Activator.CreateInstance(elementType)
			?? throw new McpToolException($"Failed to create instance of '{elementType.FullName}'");

		Tenant.GetService<INamingService>().Create(instance, (IEnumerable)collection);

		var addMethod = collection.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1)
			?? throw new McpToolException($"Collection '{collectionProperty}' does not have an Add method");

		addMethod.Invoke(collection, new[] { instance });

		Tenant.GetService<IDesignService>().Components.Update(config);

		var id = (instance as IElement)?.Id ?? Guid.Empty;
		var name = instance.GetType().GetProperty("Name")?.GetValue(instance)?.ToString();

		return new
		{
			success = true,
			elementId = id,
			name,
			elementType = elementType.FullName,
			collectionProperty
		};
	}

	internal static object DeleteElement(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var collectionProperty = args.Value<string>("collectionProperty")
			?? throw new McpToolException("collectionProperty is required");
		var elementId = ParseToken(args, "elementId");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException($"Configuration not found for component: {componentToken}");

		var prop = config.GetType().GetProperty(collectionProperty, BindingFlags.Public | BindingFlags.Instance)
			?? throw new McpToolException($"Property '{collectionProperty}' not found on '{config.GetType().Name}'");

		var collection = prop.GetValue(config)
			?? throw new McpToolException($"Collection '{collectionProperty}' is null");

		object? target = null;
		foreach (var item in (IEnumerable)collection)
		{
			if (item is IElement el && el.Id == elementId)
			{
				target = item;
				break;
			}
		}

		if (target is null)
			throw new McpToolException($"Element '{elementId}' not found in '{collectionProperty}'");

		var name = target.GetType().GetProperty("Name")?.GetValue(target)?.ToString();

		var removeMethod = collection.GetType().GetMethods().FirstOrDefault(m => m.Name == "Remove" && m.GetParameters().Length == 1)
			?? throw new McpToolException($"Collection '{collectionProperty}' does not have a Remove method");

		removeMethod.Invoke(collection, new[] { target });

		Tenant.GetService<IDesignService>().Components.Update(config);

		return new { success = true, elementId, name, collectionProperty };
	}

	internal static object WriteConfig(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var propertyName = args.Value<string>("property")
			?? throw new McpToolException("property is required");
		var valueJson = args.Value<string>("value")
			?? throw new McpToolException("value is required");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException($"Configuration not found for component: {componentToken}");

		var prop = config.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
			?? throw new McpToolException($"Property '{propertyName}' not found on '{config.GetType().Name}'. Use component_config_read to see available properties.");

		if (!prop.CanWrite)
			throw new McpToolException($"Property '{propertyName}' is read-only");

		object? converted;
		try
		{
			converted = JsonConvert.DeserializeObject(valueJson, prop.PropertyType);
		}
		catch (Exception ex)
		{
			throw new McpToolException($"Cannot convert value to {prop.PropertyType.Name}: {ex.Message}");
		}

		prop.SetValue(config, converted);

		Tenant.GetService<IDesignService>().Components.Update(config);

		return new { success = true, property = propertyName, value = valueJson };
	}

	private static Type ResolveElementType(Type collectionType, IEnumerable collection, string? elementTypeName)
	{
		if (!string.IsNullOrWhiteSpace(elementTypeName))
		{
			var t = ResolveTypeByName(elementTypeName);
			if (t is not null)
				return t;
			throw new McpToolException(
				$"Cannot resolve element type '{elementTypeName}'. " +
				"Provide a short type name (e.g. 'Operation') or a full assembly-qualified name.");
		}

		var genericArgs = collectionType.GetGenericArguments();
		if (genericArgs.Length > 0)
		{
			var argType = genericArgs[0];
			if (!argType.IsAbstract && !argType.IsInterface)
				return argType;
		}

		foreach (var item in collection)
		{
			if (item is not null)
				return item.GetType();
		}

		throw new McpToolException(
			"Cannot determine element type: the collection is empty and its generic argument is an interface. " +
			"Pass 'elementType' with the concrete type name visible in component_config_read on a similar component " +
			"(e.g. 'TomPIT.MicroServices.Apis.Operation, TomPIT.MicroServices').");
	}

	private static Type? ResolveTypeByName(string typeName)
	{
		var t = Type.GetType(typeName, throwOnError: false);
		if (t is not null)
			return t;

		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			t = asm.GetType(typeName, throwOnError: false);
			if (t is not null)
				return t;
		}

		return null;
	}

	private static Guid ParseToken(JObject args, string key)
	{
		var raw = args.Value<string>(key) ?? throw new McpToolException($"{key} is required");
		return Guid.TryParse(raw, out var g) ? g : throw new McpToolException($"{key} is not a valid GUID");
	}

	internal static IEnumerable<McpTool> Definitions()
	{
		return new List<McpTool>
		{
			new McpTool
			{
				Name = "component_element_create",
				Description = "Add a new element to a component's configuration collection (e.g. an API operation, model query, or subscription event). Use component_config_read first to identify the collection property name and existing element types. The element is auto-named by the platform.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "The component GUID token" },
						["collectionProperty"] = new() { Type = "string", Description = "Name of the collection property on the configuration (e.g. 'Operations', 'Queries', 'Workers', 'Events')" },
						["elementType"] = new() { Type = "string", Description = "Optional: concrete type name for the element. Only required when the collection is empty. Read component_config_read on a similar component to find the type (e.g. 'TomPIT.MicroServices.Apis.Operation, TomPIT.MicroServices')." }
					},
					Required = new List<string> { "componentToken", "collectionProperty" }
				}
			},
			new McpTool
			{
				Name = "component_element_delete",
				Description = "Remove an element from a component's configuration collection by its element Id. Use component_config_read to find element Ids.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "The component GUID token" },
						["collectionProperty"] = new() { Type = "string", Description = "Name of the collection property on the configuration (e.g. 'Operations', 'Queries')" },
						["elementId"] = new() { Type = "string", Description = "GUID Id of the element to remove (from component_config_read output)" }
					},
					Required = new List<string> { "componentToken", "collectionProperty", "elementId" }
				}
			},
			new McpTool
			{
				Name = "component_config_write",
				Description = "Update a scalar property on a component's configuration object (e.g. Scope, ConnectionString, Timeout). For adding or removing collection items use component_element_create or component_element_delete. Value must be valid JSON for the target property type.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "The component GUID token" },
						["property"] = new() { Type = "string", Description = "Property name on the configuration object (case-sensitive, e.g. 'Scope', 'ConnectionString', 'Timeout')" },
						["value"] = new() { Type = "string", Description = "JSON-encoded value matching the property type (e.g. '\"Public\"' for string, '2' for int/enum, 'true' for bool)" }
					},
					Required = new List<string> { "componentToken", "property", "value" }
				}
			}
		};
	}
}
