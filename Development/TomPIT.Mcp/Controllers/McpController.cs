using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Mcp.Dispatch;
using TomPIT.Mcp.Protocol;
using TomPIT.Mcp.Tools;

namespace TomPIT.Mcp.Controllers;

[Authorize(Policy = "Implement Micro Service")]
[Route("mcp")]
public class McpController : Controller
{
	[HttpGet]
	public IActionResult Info()
	{
		return Ok(new
		{
			server = "TomPIT.connected MCP",
			version = "6.1",
			protocol = "2024-11-05",
			transport = "http"
		});
	}

	[HttpPost]
	public async Task<IActionResult> Handle()
	{
		string body;

		using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
			body = await reader.ReadToEndAsync();

		JsonRpcRequest? request;

		try
		{
			request = JsonConvert.DeserializeObject<JsonRpcRequest>(body);

			if (request is null)
				return BadRequest(ErrorResponse(null, JsonRpcErrorCodes.ParseError, "Failed to parse JSON-RPC request"));
		}
		catch (Exception ex)
		{
			return BadRequest(ErrorResponse(null, JsonRpcErrorCodes.ParseError, ex.Message));
		}

		// notifications (no id) don't need a response
		if (request.Id is null && IsNotification(request.Method))
		{
			try { McpDispatcher.Handle(request); }
			catch { /* swallow notification errors */ }

			return NoContent();
		}

		try
		{
			var baseUrl = $"{Request.Scheme}://{Request.Host}";
			var result = McpDispatcher.Handle(request, baseUrl);

			return Json(new JsonRpcResponse
			{
				Jsonrpc = "2.0",
				Id = request.Id,
				Result = result
			}, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.None
			});
		}
		catch (McpMethodNotFoundException ex)
		{
			return StatusCode(200, ErrorResponse(request.Id, JsonRpcErrorCodes.MethodNotFound, ex.Message));
		}
		catch (McpInvalidParamsException ex)
		{
			return StatusCode(200, ErrorResponse(request.Id, JsonRpcErrorCodes.InvalidParams, ex.Message));
		}
		catch (Exception ex)
		{
			return StatusCode(200, ErrorResponse(request.Id, JsonRpcErrorCodes.InternalError, ex.Message));
		}
	}

	[HttpGet("tool-descriptions")]
	[AllowAnonymous]
	public IActionResult ToolDescriptions()
	{
		var result = McpDispatcher.AllTools().ToDictionary(
			t => t.Name,
			t => (object)new
			{
				description = t.Description,
				properties = t.InputSchema.Properties
					.Where(p => p.Value.Description is not null)
					.ToDictionary(p => p.Key, p => p.Value.Description)
			}
		);

		return Json(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });
	}

	[HttpGet("guide")]
	[AllowAnonymous]
	public IActionResult Guide()
	{
		return Content(ConnectedGuide.Content, "text/markdown", Encoding.UTF8);
	}

	private static bool IsNotification(string method) =>
		method is "initialized" or "notifications/cancelled" or "notifications/progress";

	private static JsonRpcError ErrorResponse(JToken? id, int code, string message) => new()
	{
		Jsonrpc = "2.0",
		Id = id,
		Error = new JsonRpcErrorBody { Code = code, Message = message }
	};

	private IActionResult Json(object value, JsonSerializerSettings settings)
	{
		var json = JsonConvert.SerializeObject(value, settings);
		return Content(json, "application/json", Encoding.UTF8);
	}
}
