using System;
using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PackageBlob : IPackageBlob
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
		[JsonProperty(PropertyName = "microService")]
		public Guid MicroService { get; set; }
		[JsonProperty(PropertyName = "version")]
		public int Version { get; set; }
		[JsonProperty(PropertyName = "topic")]
		public string Topic { get; set; }
	}
}
