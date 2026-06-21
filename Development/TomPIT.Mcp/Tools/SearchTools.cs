using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.Mcp.Protocol;
using TomPIT.Reflection;

namespace TomPIT.Mcp.Tools;

internal static class SearchTools
{
	internal static object Search(JObject args)
	{
		var msIdentifier = args.Value<string>("microService") ?? throw new McpToolException("microService is required");
		var pattern = args.Value<string>("pattern") ?? throw new McpToolException("pattern is required");
		var category = args.Value<string>("category");
		var useRegex = args.Value<bool?>("regex") ?? false;
		var maxResults = Math.Min(args.Value<int?>("maxResults") ?? 50, 200);

		var ms = Tenant.GetService<IMicroServiceService>().SelectByUrl(msIdentifier)
			?? Tenant.GetService<IMicroServiceService>().Select(msIdentifier)
			?? throw new McpToolException($"MicroService not found: '{msIdentifier}'");

		var components = string.IsNullOrWhiteSpace(category)
			? Tenant.GetService<IComponentService>().QueryComponents(ms.Token)
			: Tenant.GetService<IComponentService>().QueryComponents(ms.Token, category);

		Regex? regex = null;
		if (useRegex)
		{
			try { regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline); }
			catch (Exception ex) { throw new McpToolException($"Invalid regex: {ex.Message}"); }
		}

		var results = new List<object>();

		foreach (var component in components)
		{
			if (results.Count >= maxResults)
				break;

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);
			if (config is null)
				continue;

			var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

			foreach (var text in texts)
			{
				var source = Tenant.GetService<IComponentService>().SelectText(ms.Token, text);
				if (string.IsNullOrEmpty(source))
					continue;

				var filePath = ComponentTools.SourceFilePath(component.MicroService, text);

				var matches = useRegex
					? FindRegexMatches(source, regex!)
					: FindMatches(source, pattern);

				if (matches.Count == 0)
					continue;

				results.Add(new
				{
					componentToken = component.Token,
					component = component.Name,
					category = component.Category,
					elementName = Path.GetFileNameWithoutExtension(text.FileName),
					filePath,
					matches
				});

				if (results.Count >= maxResults)
					break;
			}
		}

		return new
		{
			pattern,
			microService = ms.Url,
			totalMatches = results.Count,
			truncated = results.Count >= maxResults,
			results
		};
	}

	private static List<object> FindMatches(string source, string pattern)
	{
		var matches = new List<object>();
		var lines = source.Split('\n');

		for (var i = 0; i < lines.Length; i++)
		{
			if (lines[i].Contains(pattern, StringComparison.OrdinalIgnoreCase))
				matches.Add(new { line = i + 1, text = lines[i].TrimEnd() });
		}

		return matches;
	}

	private static List<object> FindRegexMatches(string source, Regex regex)
	{
		var matches = new List<object>();
		var lines = source.Split('\n');

		for (var i = 0; i < lines.Length; i++)
		{
			if (regex.IsMatch(lines[i]))
				matches.Add(new { line = i + 1, text = lines[i].TrimEnd() });
		}

		return matches;
	}

	internal static IEnumerable<McpTool> Definitions()
	{
		return new List<McpTool>
		{
			new McpTool
			{
				Name = "component_search",
				Description = "Search source code across all components in a microservice. Returns matching component names, element names, and line numbers. Use this to find symbol usages, locate implementations, or navigate an unfamiliar codebase.",
				InputSchema = new()
				{
					Type = "object",
					Properties = new()
					{
						["microService"] = new() { Type = "string", Description = "Microservice URL slug or name to search within" },
						["pattern"] = new() { Type = "string", Description = "Text to search for (case-insensitive). Pass 'regex: true' to treat as a regular expression." },
						["category"] = new() { Type = "string", Description = "Optional: limit search to a component category (e.g. 'Api', 'Script', 'Model')" },
						["regex"] = new() { Type = "boolean", Description = "If true, treat pattern as a regular expression (default: false)" },
						["maxResults"] = new() { Type = "integer", Description = "Maximum number of matching elements to return (default: 50, max: 200)" }
					},
					Required = new List<string> { "microService", "pattern" }
				}
			}
		};
	}
}
