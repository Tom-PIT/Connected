using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Mcp.Protocol;
using TomPIT.Mcp.Tools;

namespace TomPIT.Mcp.Dispatch;

internal static class McpDispatcher
{
	internal static object Handle(JsonRpcRequest request)
	{
		return request.Method switch
		{
			"initialize" => HandleInitialize(request),
			"initialized" => new { },
			"ping" => new { },
			"tools/list" => HandleToolsList(),
			"tools/call" => HandleToolCall(request),
			"resources/list" => HandleResourcesList(),
			"resources/read" => HandleResourceRead(request),
			"prompts/list" => HandlePromptsList(),
			"prompts/get" => HandlePromptsGet(request),
			_ => throw new McpMethodNotFoundException(request.Method)
		};
	}

	private static McpInitializeResult HandleInitialize(JsonRpcRequest request)
	{
		return new McpInitializeResult
		{
			ProtocolVersion = "2024-11-05",
			ServerInfo = new McpServerInfo
			{
				Name = "TomPIT.connected MCP",
				Version = "6.1"
			},
			Capabilities = new McpCapabilities
			{
				Tools = new McpToolsCapability { ListChanged = false },
				Resources = new McpResourcesCapability { Subscribe = false, ListChanged = false },
				Prompts = new McpPromptsCapability { ListChanged = false }
			}
		};
	}

	private static McpToolsListResult HandleToolsList()
	{
		return new McpToolsListResult
		{
			Tools = AllTools().ToList()
		};
	}

	private static McpToolCallResult HandleToolCall(JsonRpcRequest request)
	{
		var p = request.Params as JObject ?? (request.Params is not null ? JObject.FromObject(request.Params) : new JObject());
		var toolName = p.Value<string>("name") ?? throw new McpInvalidParamsException("name is required for tools/call");
		var arguments = p["arguments"] as JObject ?? new JObject();

		try
		{
			var result = toolName switch
			{
				"microservice_list" => MicroServiceTools.List(),
				"microservice_get" => MicroServiceTools.Get(arguments),
				"microservice_references" => MicroServiceTools.GetReferences(arguments),
				"component_list" => ComponentTools.ListComponents(arguments),
				"folder_list" => ComponentTools.ListFolders(arguments),
				"component_source_read" => ComponentTools.ReadSource(arguments),
				"component_source_write" => ComponentTools.WriteSource(arguments),
				"component_config_read" => ConfigurationTools.ReadConfig(arguments),
				"component_types_list" => ConfigurationTools.ListAddableTypes(arguments),
				"component_create" => ConfigurationTools.CreateComponent(arguments),
				"component_delete" => ConfigurationTools.DeleteComponent(arguments),
				"component_rename" => ConfigurationTools.RenameComponent(arguments),
				"folder_create" => ConfigurationTools.CreateFolder(arguments),
				"api_invoke" => ApiTools.Invoke(arguments),
				"component_dependencies" => DependencyTools.Analyze(arguments),
				"script_load_resolve" => DependencyTools.ResolveLoadPath(arguments),
				_ => throw new McpToolException($"Unknown tool: {toolName}")
			};

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);

			return new McpToolCallResult
			{
				Content = new List<McpContent> { new McpContent { Type = "text", Text = json } }
			};
		}
		catch (McpToolException ex)
		{
			return new McpToolCallResult
			{
				IsError = true,
				Content = new List<McpContent> { new McpContent { Type = "text", Text = ex.Message } }
			};
		}
		catch (Exception ex)
		{
			return new McpToolCallResult
			{
				IsError = true,
				Content = new List<McpContent> { new McpContent { Type = "text", Text = $"Error: {ex.Message}" } }
			};
		}
	}

	private static McpResourcesListResult HandleResourcesList()
	{
		var microServices = TomPIT.Tenant.GetService<TomPIT.ComponentModel.IMicroServiceService>().Query();

		var resources = new List<McpResource>
		{
			new McpResource
			{
				Uri = "tompit://guide",
				Name = "TomPIT Connected — Platform Guide",
				Description = "Comprehensive guide to TomPIT Connected architecture, component model, #load resolution, dependencies, and effective tool use patterns",
				MimeType = "text/markdown"
			},
			new McpResource
			{
				Uri = "tompit://microservices",
				Name = "All Microservices",
				Description = "Complete list of microservices on this instance",
				MimeType = "application/json"
			}
		};

		foreach (var ms in microServices)
		{
			resources.Add(new McpResource
			{
				Uri = $"tompit://{ms.Url}/components",
				Name = $"{ms.Name} – Components",
				Description = $"All components in the {ms.Name} microservice",
				MimeType = "application/json"
			});
		}

		return new McpResourcesListResult { Resources = resources };
	}

	private static McpResourceReadResult HandleResourceRead(JsonRpcRequest request)
	{
		var p = request.Params as JObject ?? new JObject();
		var uri = p.Value<string>("uri") ?? throw new McpInvalidParamsException("uri is required for resources/read");

		string content;

		if (uri == "tompit://guide")
		{
			content = ConnectedGuide.Content;
		}
		else if (uri == "tompit://microservices")
		{
			var list = MicroServiceTools.List();
			content = Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
		}
		else if (uri.StartsWith("tompit://") && uri.EndsWith("/components"))
		{
			var msUrl = uri["tompit://".Length..^"/components".Length];
			var args = new JObject { ["microService"] = msUrl };
			var list = ComponentTools.ListComponents(args);
			content = Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
		}
		else
		{
			throw new McpInvalidParamsException($"Unknown resource URI: {uri}");
		}

		return new McpResourceReadResult
		{
			Contents = new List<McpResourceContent> { new McpResourceContent { Uri = uri, MimeType = "application/json", Text = content } }
		};
	}

	private static McpPromptsListResult HandlePromptsList()
	{
		return new McpPromptsListResult
		{
			Prompts = new List<McpPrompt>
			{
				new McpPrompt
				{
					Name = "implement_api_operation",
					Description = "Generate the implementation body for a TomPIT API operation given its context",
					Arguments = new List<McpPromptArgument>
					{
						new McpPromptArgument { Name = "componentToken", Description = "Token of the API component", Required = true },
						new McpPromptArgument { Name = "operationName", Description = "Name of the operation to implement", Required = true }
					}
				},
				new McpPrompt
				{
					Name = "review_component",
					Description = "Review a component's source code and suggest improvements",
					Arguments = new List<McpPromptArgument>
					{
						new McpPromptArgument { Name = "componentToken", Description = "Token of the component to review", Required = true }
					}
				},
				new McpPrompt
				{
					Name = "document_api",
					Description = "Generate documentation for an API component and its operations",
					Arguments = new List<McpPromptArgument>
					{
						new McpPromptArgument { Name = "componentToken", Description = "Token of the API component", Required = true }
					}
				}
			}
		};
	}

	private static McpPromptGetResult HandlePromptsGet(JsonRpcRequest request)
	{
		var p = request.Params as JObject ?? new JObject();
		var name = p.Value<string>("name") ?? throw new McpInvalidParamsException("name is required for prompts/get");
		var promptArgs = p["arguments"] as JObject ?? new JObject();

		return name switch
		{
			"implement_api_operation" => BuildImplementApiPrompt(promptArgs),
			"review_component" => BuildReviewComponentPrompt(promptArgs),
			"document_api" => BuildDocumentApiPrompt(promptArgs),
			_ => throw new McpInvalidParamsException($"Unknown prompt: {name}")
		};
	}

	private static McpPromptGetResult BuildImplementApiPrompt(JObject args)
	{
		var tokenRaw = args.Value<string>("componentToken") ?? string.Empty;
		var opName = args.Value<string>("operationName") ?? string.Empty;

		var contextInfo = string.Empty;

		if (Guid.TryParse(tokenRaw, out var token))
		{
			var config = TomPIT.Tenant.GetService<TomPIT.ComponentModel.IComponentService>().SelectConfiguration(token);
			contextInfo = config is not null
				? Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented,
					new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore })
				: "(configuration not found)";
		}

		return new McpPromptGetResult
		{
			Description = $"Implement the '{opName}' operation",
			Messages = new List<McpPromptMessage>
			{
				new McpPromptMessage
				{
					Role = "user",
					Content = new McpContent
					{
						Type = "text",
						Text = $"I need you to implement the '{opName}' API operation in TomPIT.connected.\n\nComponent configuration:\n```json\n{contextInfo}\n```\n\nThe implementation must be a C# script class that inherits from the appropriate base (e.g. Api<T> for operations with return value, or Api for void operations). Follow the existing patterns in the codebase."
					}
				}
			}
		};
	}

	private static McpPromptGetResult BuildReviewComponentPrompt(JObject args)
	{
		var tokenRaw = args.Value<string>("componentToken") ?? string.Empty;
		var sources = new List<string>();

		if (Guid.TryParse(tokenRaw, out var token))
		{
			var component = TomPIT.Tenant.GetService<TomPIT.ComponentModel.IComponentService>().SelectComponent(token);
			var ms = component is not null ? TomPIT.Tenant.GetService<TomPIT.ComponentModel.IMicroServiceService>().Select(component.MicroService) : null;
			var config = TomPIT.Tenant.GetService<TomPIT.ComponentModel.IComponentService>().SelectConfiguration(token);

			if (config is not null && ms is not null)
			{
				var texts = TomPIT.Tenant.GetService<TomPIT.Reflection.IDiscoveryService>().Configuration.Query<TomPIT.ComponentModel.IText>(config);
				foreach (var t in texts)
				{
					var src = TomPIT.Tenant.GetService<TomPIT.ComponentModel.IComponentService>().SelectText(ms.Token, t);
					if (!string.IsNullOrWhiteSpace(src))
						sources.Add($"### {System.IO.Path.GetFileNameWithoutExtension(t.FileName)}\n```csharp\n{src}\n```");
				}
			}
		}

		return new McpPromptGetResult
		{
			Description = "Review component source code",
			Messages = new List<McpPromptMessage>
			{
				new McpPromptMessage
				{
					Role = "user",
					Content = new McpContent
					{
						Type = "text",
						Text = $"Please review the following TomPIT component and suggest improvements, identify potential bugs, and check for best practices:\n\n{string.Join("\n\n", sources)}"
					}
				}
			}
		};
	}

	private static McpPromptGetResult BuildDocumentApiPrompt(JObject args)
	{
		var tokenRaw = args.Value<string>("componentToken") ?? string.Empty;

		return new McpPromptGetResult
		{
			Description = "Document API component",
			Messages = new List<McpPromptMessage>
			{
				new McpPromptMessage
				{
					Role = "user",
					Content = new McpContent
					{
						Type = "text",
						Text = $"Use component_source_read with componentToken '{tokenRaw}' to read the API source, then generate clear markdown documentation for each operation including: purpose, input parameters, return value, and example usage."
					}
				}
			}
		};
	}

	private static IEnumerable<McpTool> AllTools()
	{
		foreach (var t in MicroServiceTools.Definitions()) yield return t;
		foreach (var t in ComponentTools.Definitions()) yield return t;
		foreach (var t in ConfigurationTools.Definitions()) yield return t;
		foreach (var t in ApiTools.Definitions()) yield return t;
		foreach (var t in DependencyTools.Definitions()) yield return t;
	}
}

public class McpMethodNotFoundException : Exception
{
	public McpMethodNotFoundException(string method) : base($"Method not found: {method}") { }
}

public class McpInvalidParamsException : Exception
{
	public McpInvalidParamsException(string message) : base(message) { }
}
