using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
{
	public class Blob
	{
		[JsonProperty(PropertyName = "content")]
		public string Content { get; set; }
		[JsonProperty(PropertyName = "fileName")]
		public string FileName { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
		[JsonProperty(PropertyName = "type")]
		public int Type { get; set; }
		[JsonProperty(PropertyName = "contentType")]
		public string ContentType { get; set; }
		[JsonProperty(PropertyName = "primaryKey")]
		public string PrimaryKey { get; set; }
		[JsonProperty(PropertyName = "service")]
		public Guid Service { get; set; }
		[JsonProperty(PropertyName = "version")]
		public int Version { get; set; }
		[JsonProperty(PropertyName = "topic")]
		public string Topic { get; set; }
	}
}
