using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageMetaData : IPackageMetaData
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
		[JsonProperty(PropertyName = "account")]
		public Guid Account { get; set; }
		[JsonProperty(PropertyName = "created")]
		public DateTime Created { get; set; }
		[JsonProperty(PropertyName = "price")]
		public double Price { get; set; }
		[JsonProperty(PropertyName = "shellVersion")]
		public string ShellVersion { get; set; }
		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }
		[JsonProperty(PropertyName = "projectUrl")]
		public string ProjectUrl { get; set; }
		[JsonProperty(PropertyName = "licenseUrl")]
		public string LicenseUrl { get; set; }
		[JsonProperty(PropertyName = "tags")]
		public string Tags { get; set; }
		[JsonProperty(PropertyName = "imageUrl")]
		public string ImageUrl { get; set; }
		[JsonProperty(PropertyName = "trial")]
		public bool Trial { get; set; }
		[JsonProperty(PropertyName = "trialPeriod")]
		public int TrialPeriod { get; set; }
		[JsonProperty(PropertyName = "licenses")]
		public string Licenses { get; set; }
	}
}
