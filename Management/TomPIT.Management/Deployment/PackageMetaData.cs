using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
{
	public enum PackageScope
	{
		Public = 1,
		Private = 2
	}

	public class PackageMetaData
	{
		[JsonProperty(PropertyName = "id")]
		public Guid Id { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }
		[JsonProperty(PropertyName = "version")]
		public string Version { get; set; }
		[JsonProperty(PropertyName = "scope")]
		public PackageScope Scope { get; set; }
		[JsonProperty(PropertyName = "publisher")]
		public Guid Publisher { get; set; }
		[JsonProperty(PropertyName = "created")]
		public DateTime Created { get; set; }
		[JsonProperty(PropertyName = "price")]
		public double Price { get; set; }
		[JsonProperty(PropertyName = "shellVersion")]
		public string ShellVersion { get; set; }
		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
		[JsonProperty(PropertyName = "licenseUrl")]
		public string LicenseUrl { get; set; }
		[JsonProperty(PropertyName = "tags")]
		public string Tags { get; set; }
		[JsonProperty(PropertyName = "imageUrl")]
		public string ImageUrl { get; set; }
	}
}
