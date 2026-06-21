using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design;
using TomPIT.Mcp.Protocol;

namespace TomPIT.Mcp.Tools;

internal static class StringTableTools
{
	internal static object SetString(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var key = args.Value<string>("key") ?? throw new McpToolException("key is required");
		var defaultValue = args.Value<string>("defaultValue") ?? string.Empty;
		var isLocalizable = args.Value<bool?>("isLocalizable") ?? true;

		var config = ResolveConfig(componentToken);

		var existing = config.Strings.FirstOrDefault(s =>
			string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));

		if (existing is not null)
		{
			existing.GetType().GetProperty("DefaultValue")?.SetValue(existing, defaultValue);
			existing.GetType().GetProperty("IsLocalizable")?.SetValue(existing, isLocalizable);
		}
		else
		{
			var concreteType = FindConcreteType(typeof(IStringResource))
				?? throw new McpToolException("Cannot resolve concrete type for IStringResource. Ensure TomPIT.MicroServices is loaded.");

			var instance = (IStringResource)(Activator.CreateInstance(concreteType)
				?? throw new McpToolException("Failed to create IStringResource instance"));

			concreteType.GetProperty("Key")?.SetValue(instance, key);
			concreteType.GetProperty("DefaultValue")?.SetValue(instance, defaultValue);
			concreteType.GetProperty("IsLocalizable")?.SetValue(instance, isLocalizable);

			config.Strings.Add(instance);
		}

		Tenant.GetService<IDesignService>().Components.Update(config);

		return new { success = true, key, defaultValue, isLocalizable, created = existing is null };
	}

	internal static object DeleteString(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var key = args.Value<string>("key") ?? throw new McpToolException("key is required");

		var config = ResolveConfig(componentToken);

		var existing = config.Strings.FirstOrDefault(s =>
			string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase))
			?? throw new McpToolException($"String resource '{key}' not found");

		config.Strings.Remove(existing);

		Tenant.GetService<IDesignService>().Components.Update(config);

		return new { success = true, key };
	}

	internal static object SetTranslation(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");
		var key = args.Value<string>("key") ?? throw new McpToolException("key is required");
		var lcid = args.Value<int?>("lcid") ?? throw new McpToolException("lcid is required");
		var value = args.Value<string>("value") ?? string.Empty;

		var config = ResolveConfig(componentToken);

		var resource = config.Strings.FirstOrDefault(s =>
			string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase))
			?? throw new McpToolException($"String resource '{key}' not found. Create it first with string_table_set.");

		resource.UpdateTranslation(lcid, value);

		Tenant.GetService<IDesignService>().Components.Update(config);

		return new { success = true, key, lcid, value = string.IsNullOrEmpty(value) ? "(removed)" : value };
	}

	private static IStringTableConfiguration ResolveConfig(Guid componentToken)
	{
		return Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			as IStringTableConfiguration
			?? throw new McpToolException("Component is not a StringTable");
	}

	private static Type? FindConcreteType(Type interfaceType)
	{
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				var match = asm.GetTypes().FirstOrDefault(t =>
					!t.IsAbstract && !t.IsInterface && interfaceType.IsAssignableFrom(t));

				if (match is not null)
					return match;
			}
			catch { /* skip assemblies that fail reflection */ }
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
				Name = "string_table_set",
				Description = "Create or update a string resource entry in a StringTable component. Matches by key (case-insensitive). Use component_config_read to inspect existing entries.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "StringTable component GUID token" },
						["key"] = new() { Type = "string", Description = "Resource key (unique identifier within the table)" },
						["defaultValue"] = new() { Type = "string", Description = "Default value / fallback text" },
						["isLocalizable"] = new() { Type = "boolean", Description = "Whether this entry can be translated (default: true)" }
					},
					Required = new List<string> { "componentToken", "key" }
				}
			},
			new McpTool
			{
				Name = "string_table_delete",
				Description = "Remove a string resource entry from a StringTable component by its key.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "StringTable component GUID token" },
						["key"] = new() { Type = "string", Description = "Resource key to delete" }
					},
					Required = new List<string> { "componentToken", "key" }
				}
			},
			new McpTool
			{
				Name = "string_table_translate",
				Description = "Add or update a translation for a string resource. Pass an empty value to remove the translation for that locale. The lcid is the Windows locale identifier (e.g. 1033 = en-US, 1031 = de-DE).",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["componentToken"] = new() { Type = "string", Description = "StringTable component GUID token" },
						["key"] = new() { Type = "string", Description = "Resource key to translate" },
						["lcid"] = new() { Type = "integer", Description = "Windows locale ID (e.g. 1033 for en-US, 1031 for de-DE)" },
						["value"] = new() { Type = "string", Description = "Translated text. Pass empty string to remove this translation." }
					},
					Required = new List<string> { "componentToken", "key", "lcid" }
				}
			}
		};
	}
}
