using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.Mcp.Protocol;
using TomPIT.Reflection;

namespace TomPIT.Mcp.Tools;

internal static class DependencyTools
{
	// Matches: #load "microservice/script" or #load "microservice/script/file"
	private static readonly Regex LoadDirectiveRegex = new(@"#load\s+""([^""]+)""", RegexOptions.Compiled | RegexOptions.Multiline);

	// Matches: import ... from '...' or import ... from "..."
	private static readonly Regex JsImportRegex = new(@"import\s+(?:[^;]+?\s+from\s+)?['""]([^'"";]+)['""]", RegexOptions.Compiled | RegexOptions.Multiline);

	// Matches @using or @inject in Razor
	private static readonly Regex RazorUsingRegex = new(@"@using\s+([\w\.]+)", RegexOptions.Compiled | RegexOptions.Multiline);

	internal static object Analyze(JObject args)
	{
		var componentToken = ParseToken(args, "componentToken");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken)
			?? throw new McpToolException($"Component not found: {componentToken}");

		var ms = Tenant.GetService<IMicroServiceService>().Select(component.MicroService)
			?? throw new McpToolException("MicroService not found");

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken)
			?? throw new McpToolException("Configuration not found");

		var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

		var allDependencies = new List<object>();

		foreach (var text in texts)
		{
			var source = Tenant.GetService<IComponentService>().SelectText(ms.Token, text);

			if (string.IsNullOrWhiteSpace(source))
				continue;

			var elementName = System.IO.Path.GetFileNameWithoutExtension(text.FileName);
			var ext = System.IO.Path.GetExtension(text.FileName)?.ToLowerInvariant();

			var deps = ext switch
			{
				".csx" or ".cs" => ExtractCSharpDependencies(source, ms, component),
				".js" or ".ts" or ".mjs" => ExtractJsDependencies(source),
				".cshtml" or ".html" => ExtractRazorDependencies(source),
				_ => []
			};

			if (deps.Count > 0)
				allDependencies.Add(new { elementName, ext, dependencies = deps });
		}

		// Also include IDiscoveryService-tracked configuration dependencies (GUIDs of linked components)
		var configDeps = Tenant.GetService<IDiscoveryService>().Configuration.QueryDependencies(config);

		return new
		{
			component = component.Name,
			category = component.Category,
			microService = ms.Url,
			sourceDependencies = allDependencies,
			configurationDependencies = configDeps.Select(dep =>
			{
				var depComponent = Tenant.GetService<IComponentService>().SelectComponent(dep);
				return new
				{
					token = dep,
					name = depComponent?.Name,
					category = depComponent?.Category
				};
			}).ToList()
		};
	}

	private static List<object> ExtractCSharpDependencies(string source, IMicroService currentMs, IComponent component)
	{
		var deps = new List<object>();

		// Parse using Roslyn to get #load directives precisely
		var tree = CSharpSyntaxTree.ParseText(source,
			new CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest,
				Microsoft.CodeAnalysis.DocumentationMode.None,
				Microsoft.CodeAnalysis.SourceCodeKind.Script));

		if (tree.TryGetRoot(out _))
		{
			var root = tree.GetCompilationUnitRoot();
			var loadDirectives = root.GetLoadDirectives();

			foreach (var directive in loadDirectives)
			{
				var path = directive.File.ValueText;

				if (string.IsNullOrWhiteSpace(path))
					continue;

				var tokens = path.Split('/');
				string? referencedMs = null;
				string? referencedScript = null;
				string? referencedFile = null;

				if (tokens.Length >= 2)
				{
					referencedMs = tokens[0];
					referencedScript = tokens[1];
					referencedFile = tokens.Length > 2 ? tokens[2] : null;
				}
				else
				{
					referencedMs = currentMs.Name;
					referencedScript = tokens[0];
				}

				// Try to resolve the referenced component
				var targetMs = Tenant.GetService<IMicroServiceService>().Select(referencedMs ?? string.Empty);
				IComponent? targetComponent = null;

				if (targetMs is not null && referencedScript is not null)
					targetComponent = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(targetMs.Token, ComponentCategories.NameSpacePublicScript, referencedScript);

				deps.Add(new
				{
					kind = "csharp_load",
					path,
					referencedMicroService = referencedMs,
					referencedComponent = referencedScript,
					referencedElement = referencedFile,
					resolved = targetComponent is not null,
					resolvedToken = targetComponent?.Token
				});
			}
		}

		// Fallback regex for any edge cases Roslyn misses
		foreach (Match match in LoadDirectiveRegex.Matches(source))
		{
			var path = match.Groups[1].Value;
			if (!deps.Any(d => ((dynamic)d).path == path))
			{
				deps.Add(new
				{
					kind = "csharp_load",
					path,
					referencedMicroService = (string?)null,
					referencedComponent = (string?)null,
					referencedElement = (string?)null,
					resolved = false,
					resolvedToken = (Guid?)null
				});
			}
		}

		return deps;
	}

	private static List<object> ExtractJsDependencies(string source)
	{
		var deps = new List<object>();

		foreach (Match match in JsImportRegex.Matches(source))
		{
			var importPath = match.Groups[1].Value;
			deps.Add(new
			{
				kind = "js_import",
				path = importPath,
				isRelative = importPath.StartsWith("./") || importPath.StartsWith("../"),
				isAbsolute = importPath.StartsWith("/"),
				isPackage = !importPath.StartsWith(".") && !importPath.StartsWith("/")
			});
		}

		return deps;
	}

	private static List<object> ExtractRazorDependencies(string source)
	{
		var deps = new List<object>();

		foreach (Match match in RazorUsingRegex.Matches(source))
		{
			deps.Add(new
			{
				kind = "razor_using",
				nameSpace = match.Groups[1].Value
			});
		}

		// Also check for JS imports inside the Razor markup
		foreach (Match match in JsImportRegex.Matches(source))
		{
			deps.Add(new
			{
				kind = "js_import",
				path = match.Groups[1].Value
			});
		}

		return deps;
	}

	internal static IEnumerable<McpTool> Definitions() =>
	[
		new McpTool
		{
			Name = "component_dependencies",
			Description = "Analyze a component's dependencies. For C# scripts extracts #load directives and resolves them to components. For JS/TS extracts import statements. For Razor views extracts @using and JS imports. Also returns configuration-level dependencies tracked by the runtime.",
			InputSchema = new()
			{
				Type = "object",
				Properties = new()
				{
					["componentToken"] = new() { Type = "string", Description = "The component GUID token" }
				},
				Required = ["componentToken"]
			}
		}
	];

	private static Guid ParseToken(JObject args, string key)
	{
		var raw = args.Value<string>(key) ?? throw new McpToolException($"{key} is required");
		return Guid.TryParse(raw, out var g) ? g : throw new McpToolException($"{key} is not a valid GUID");
	}
}
