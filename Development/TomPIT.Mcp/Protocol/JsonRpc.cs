using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TomPIT.Mcp.Protocol;

public class JsonRpcRequest
{
	[JsonProperty("jsonrpc")]
	public string Jsonrpc { get; set; } = "2.0";

	[JsonProperty("id")]
	public JToken? Id { get; set; }

	[JsonProperty("method")]
	public string Method { get; set; } = string.Empty;

	[JsonProperty("params")]
	public JToken? Params { get; set; }
}

public class JsonRpcResponse
{
	[JsonProperty("jsonrpc")]
	public string Jsonrpc { get; set; } = "2.0";

	[JsonProperty("id")]
	public JToken? Id { get; set; }

	[JsonProperty("result")]
	public object? Result { get; set; }
}

public class JsonRpcError
{
	[JsonProperty("jsonrpc")]
	public string Jsonrpc { get; set; } = "2.0";

	[JsonProperty("id")]
	public JToken? Id { get; set; }

	[JsonProperty("error")]
	public JsonRpcErrorBody Error { get; set; } = new();
}

public class JsonRpcErrorBody
{
	[JsonProperty("code")]
	public int Code { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; } = string.Empty;

	[JsonProperty("data")]
	public object? Data { get; set; }
}

public static class JsonRpcErrorCodes
{
	public const int ParseError = -32700;
	public const int InvalidRequest = -32600;
	public const int MethodNotFound = -32601;
	public const int InvalidParams = -32602;
	public const int InternalError = -32603;
}
